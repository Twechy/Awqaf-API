using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Utils.Auth.JwtHelper;
using Utils.Others;
using Utils.Vm;

namespace Utils.Auth
{
    public abstract class PermissionManagementBaseService<TIdentity, TIdentityVM>
        where TIdentity : UserIdentity where TIdentityVM : UserIdentityVM
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IOptions<JWTOptions> _option;
        protected readonly IMapper _mapper;
        protected readonly DbContext _dbContext;

        public PermissionManagementBaseService(DbContext dbContext,
            IMapper mapper,
            IDistributedCache distributedCache, IOptions<JWTOptions> option)

        {
            _dbContext = dbContext;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _option = option;
        }


        public virtual async Task<bool> Add(TIdentityVM user, long defaultCreds = 0)
        {
            var userModel = _mapper.Map<TIdentityVM, TIdentity>(user);
            userModel.Creds = defaultCreds;
            _dbContext.Add(userModel);

            return await _dbContext.SaveChangesAsync() >= 1;
        }

        public virtual async Task Edit(TIdentityVM user)
        {
            var identity = await _dbContext.Set<TIdentity>().SingleOrDefaultAsync(x => x.Id == user.Id);
            identity.ExtraData = new Dictionary<string, string>();
            _mapper.Map(user, identity);

            await invalidateCache(identity.Id);
            await _dbContext.SaveChangesAsync();
        }


        private async Task<int> GetCount(Expression<Func<TIdentity, bool>> expression)
            => await _dbContext.Set<TIdentity>().CountAsync(expression);


        public async Task<PageResult<TIdentityVM>> FetchPage(int page, int pageSize,
            string userName = "", int identityTag = 0,
            Expression<Func<TIdentity, bool>> otherExpression = null)
        {
            var expression =
                string.IsNullOrEmpty(userName) && otherExpression != null
                    ? otherExpression
                    : x => x.UserName.Contains(userName) && (x.IdentityTag == identityTag);


            var result = (await _dbContext.Set<TIdentity>()
                .AsNoTracking()
                .Where(expression).OrderBy(x=>x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()).Select(x => _mapper.Map<TIdentity, TIdentityVM>(x)).ToList();

            return PageResult<TIdentityVM>.Create(await GetCount(expression),
                page, pageSize, result);
        }


        public async Task<TIdentity> FetchUser(int userId = -1,
            string userName = "", string password = "",
            Expression<Func<TIdentity, bool>> otherExpression = null)
        {
            Expression<Func<TIdentity, bool>> expression;

            if (userId > 0)
                expression = x => x.Id == userId;
            else if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                expression = x => x.UserName == userName && x.Pin == password && x.IsEnabled;
            else if (otherExpression != null)
                expression = otherExpression;
            else throw new AppException("you must choose condition for search ..... ");

            return await _dbContext.Set<TIdentity>().AsNoTracking().SingleOrDefaultAsync(expression);
        }

        public async Task ChangeState(int userId, bool changeStateTo, int identityTag = 0)
        {
            var user = await _dbContext.Set<TIdentity>()
                .SingleOrDefaultAsync(x => x.Id == userId && x.IdentityTag == identityTag);
            if (user == null)
                throw new AppException("Invalid User Id", "لم يتم عثور على المستخدم");

            user.IsEnabled = changeStateTo;
            await invalidateCache(userId);
            await _dbContext.SaveChangesAsync();
        }


        public virtual async Task SetPrems(int userId, long perms)
        {
            var user = await _dbContext.Set<TIdentity>().SingleOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new AppException("Invalid User Id", "لم يتم عثور على المستخدم");

            user.Creds = perms;
            await invalidateCache(userId);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangePin(int userId, string oldPin, string newPin,
            int len = 4)
        {
            var user = await _dbContext.Set<TIdentity>().SingleOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new AppException("Invalid User Id", "لم يتم عثور على المستخدم");

            if (string.IsNullOrEmpty(oldPin) || user.Pin != oldPin)
                throw new AppException("Invalid old pin", "كلمة مرور قديمة غير مطابقة");
            if (string.IsNullOrEmpty(newPin) || newPin.Length < len)
                throw new AppException("Invalid new pin",
                    string.Format("يجب ان تكون كلمة المرور {0} حروف او ارقام على الاقل ", len));

            user.Pin = newPin;
            await invalidateCache(userId);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<SingInResult<TIdentity>> SignIn(string refreshToken, int timeout = 1,
            string refreshTag = "")
        {
            var userId = await GetIdByRefreshToken(refreshToken);
            return userId > 0
                ? await SignIn(userId,
                    refreshTag: refreshTag)
                : null;
        }


        public async Task<SingInResult<TIdentity>> SignIn(int refId = -1,
            string userName = "",
            string password = "",
            string refreshTag = "")
        {
            var user = await GetUser(refId, userName, password);

            if (user != null)
                return new SingInResult<TIdentity>
                {
                    UserIdentity = user,
                    RefreshToken = await generateRefreshToken(_option.Value.TokenTimeout,
                    refreshTag, user, _option.Value.RefreshTokenTimeout)
                };

            return null;
        }

        private async Task<string> generateRefreshToken(int timeout, 
            string refreshTag, TIdentity user,
            int refreshTimeOut )
        {
            var refreshToken = Guid.NewGuid().ToString();
            refreshToken = !string.IsNullOrEmpty(refreshTag) ? $"{refreshTag}_{refreshToken}" : refreshToken;

            user.RefrechToken = refreshToken;
            user.RTValidTo = DateTime.Now.AddMinutes(timeout + refreshTimeOut);

            _distributedCache.SetString(refreshToken, user.Id.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = new TimeSpan(0, timeout + refreshTimeOut, 0)
                });
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        private async Task<TIdentity> GetUser(int refId, string userName, string password)
        {
            Expression<Func<TIdentity, bool>> expr = x => x.IsEnabled &&
                                                          x.UserName == userName && x.Pin == password;

            if (refId > 0)
                expr = x => x.IsEnabled && x.Id == refId;

            var user = await _dbContext.Set<TIdentity>()
                .SingleOrDefaultAsync(expr);
            return user;
        }

        private async Task<int> GetIdByRefreshToken(string refreshToken)
        {
            var dateNow = DateTime.Now;
            var userId = _distributedCache.GetString(refreshToken);
            if (userId == null)
                return await _dbContext.Set<TIdentity>().Where(x => x.RefrechToken == refreshToken &&
                                                                   x.RTValidTo.HasValue && x.RTValidTo > dateNow)
                    .Select(x => x.Id)
                    .SingleOrDefaultAsync();

            return int.Parse(userId);
        }

        async Task invalidateCache(int userId)
        {
            var user = await _dbContext.Set<TIdentity>().SingleOrDefaultAsync(x => x.Id == userId);
            var currentToken = user.RefrechToken;
            if (currentToken != null &&
                _distributedCache.GetString(currentToken) != null)
                _distributedCache.Remove(currentToken);
        }
    }

    public class UserIdentity
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Pin { get; set; }
        public PasswordType Type { get; set; }
        public bool IsEnabled { get; set; } = true;
        public long Creds { get; set; }
        public int IdentityTag { get; set; } = 0;
        public string RefrechToken { get; set; }
        public DateTime? RTValidTo { get; set; }
        public Dictionary<string, string> ExtraData { get; set; }
    }

    public class UserIdentityVM
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int IdentityTag { get; set; } = 0;
        public string Pin { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, string> ExtraData { get; set; }
    }

    public class SingInResult<Identity>
        where Identity : UserIdentity
    {
        public Identity UserIdentity { get; set; }
        public string RefreshToken { get; set; }
    }

    public enum PasswordType
    {
        Normal,
        OTP
    }
}

