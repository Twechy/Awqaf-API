using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Utils.Others.Blocker
{
    public static class UserBlockerExtenstion
    {
        public static IServiceCollection AddUserBlocker(this IServiceCollection serviceDescriptors)
        {
            IConfiguration config = null;
            using (var scope = serviceDescriptors.BuildServiceProvider())
            {
                config = scope.GetService<IConfiguration>();
                serviceDescriptors.Configure<BlockOptions>(config.GetSection("UserBlockOptions"));
            }

            var userBlocksConnectionString = config.GetConnectionString("UserBlockerDB");
            serviceDescriptors.AddDbContext<BlocksDBContext>(options =>
            {
                options.UseSqlServer(userBlocksConnectionString,
                    x => x.MigrationsHistoryTable("__MigrationsHistoryForUserBlocks", "migrations"));
            });

            using (var scope = serviceDescriptors.BuildServiceProvider())
                scope.GetService<BlocksDBContext>().Database.Migrate();

            serviceDescriptors.TryAddTransient<UserBlocker>();
            return serviceDescriptors;
        }
    }

    public class UserBlocker
    {
        private readonly BlocksDBContext _blocksDBContext = null;
        private readonly CacheProvider _cacheProvider;
        private IOptions<BlockOptions> _blockOptios;
        private CashLocker _locker;

        public UserBlocker(BlocksDBContext blocksDBContext,
                           CacheProvider cacheProvider,
                           IOptions<BlockOptions> options,
                           CashLocker locker)
        {
            _blocksDBContext = blocksDBContext;
            _cacheProvider = cacheProvider;
            _blockOptios = options;
            _locker = locker;
        }

        public async Task<string> IsBlocked(string userKey, bool softBlock = false)
        {
            bool isBlocked = false;
            if (softBlock)
            {
                var result = getCurrentSoftBlockState(userKey);
                isBlocked = result >= _blockOptios.Value.NumberOfRetriestOnSoftBlock;
            }
            else
            {
                isBlocked = await _blocksDBContext.UserBlocks.AnyAsync(x => x.UserKey == userKey && x.IsBlocked);
            }

            if (isBlocked)
                return softBlock ? "تم_ايقاف_هوية_الحساب_التي_تحاول_الدخول_بها_لمدة_الرجاء_الاتصال_بمركز_الخدمات_لمزيد_من_المعلومات" :
                       "تم_ايقاف_حسابك__الرجاء_الاتصال_بمركز_الخدمات_لمزيد_من_المعلومات";
            return null;
        }

        public void SoftBlock(string userKey, bool release = false)
        {
            var result = release ? 0 : getCurrentSoftBlockState(userKey) + 1;
            _cacheProvider.AddToCacheAsString(userKey, result.ToString(),
                new TimeSpan(0, _blockOptios.Value.NumberOfMinsInSoftBlock, 0));
        }

        public bool VerifyOtpLock(string otplock, int verficationCodeTimeOut = 90, int window = 1)
        {
            if (otplock == "") return false;
            //var result = _cacheProvider.GetString(otplock);
            //if (string.IsNullOrEmpty(result))
            //{
            //    _cacheProvider.AddToCacheAsString(otplock, otplock,
            //   new TimeSpan(0, 0, 20));
            //    return false;
            //}
            var timeOut = verficationCodeTimeOut * (window * 3);
            return !_locker.AcquireLock(otplock, otplock,
                TimeSpan.FromSeconds(timeOut));
        }

        public bool VerifyOtp(string otplock)
        {
            if (otplock == "") return false;
            var result = _cacheProvider.GetString(otplock);
            if (string.IsNullOrEmpty(result))
            {
                _cacheProvider.AddToCacheAsString(otplock, otplock,
                    new TimeSpan(0, 0, 20));
                return false;
            }
            return true;
        }

        public async Task<VerificationResult> Verify(string userKey, string code, bool withBlockProcedure = true, int verficationCodeTimeOut = 90, int window = 1, bool isEmail = false)
        {
            var compareResult = verifyToken(userKey, code, verficationCodeTimeOut, window, isEmail);
            if (!withBlockProcedure) return compareResult ? VerificationResult.Correct : VerificationResult.IncorrectOrExpired;

            var userBlockModel = await _blocksDBContext.UserBlocks.SingleOrDefaultAsync(x => x.UserKey == userKey);

            userBlockModel = await addNewUserIfNotExist(userKey, userBlockModel);
            if (userBlockModel.IsBlocked) return VerificationResult.Blocked;

            if (!compareResult)
                return await blockUser(userBlockModel) ? VerificationResult.Blocked : VerificationResult.IncorrectOrExpired;

            await releaseUser(userBlockModel);
            return VerificationResult.Correct;
        }

        private async Task<bool> blockUser(BlockModel userBlockModel)
        {
            userBlockModel.NumberOFRetry++;
            userBlockModel.IsBlocked = userBlockModel.NumberOFRetry >= _blockOptios.Value.NumberOfRetriestOnHardBlock;
            await _blocksDBContext.SaveChangesAsync();

            return userBlockModel.IsBlocked;
        }

        private async Task releaseUser(BlockModel userBlockModel)
        {
            userBlockModel.NumberOFRetry = 0;
            userBlockModel.IsBlocked = false;
            await _blocksDBContext.SaveChangesAsync();
        }

        private async Task<BlockModel> addNewUserIfNotExist(string userKey, BlockModel userBlockModel)
        {
            if (userBlockModel == null)
            {
                userBlockModel = new BlockModel { UserKey = userKey, IsBlocked = false, NumberOFRetry = 0 };
                _blocksDBContext.UserBlocks.Add(userBlockModel);
                await _blocksDBContext.SaveChangesAsync();
            }

            return userBlockModel;
        }

        private int getCurrentSoftBlockState(string userKey)
        {
            var result = 0;
            int.TryParse(_cacheProvider.GetString(userKey), out result);
            return result;
        }

        private bool verifyToken(string userKey, string otp, int verficationCodeTimeOut = 90, int window = 1, bool isEmail = false)
        {
            long matchedStep;
            var otpSecret = Encoding.UTF8.GetBytes("the_very_very_super_secret_key_$$$$_Alwas_secret_" + userKey.Trim() + (isEmail ? "EE" : ""));
            var otpVerifier = new OtpNet.Totp(otpSecret, verficationCodeTimeOut);
            return otpVerifier.VerifyTotp(DateTime.Now, otp, out matchedStep, new OtpNet.VerificationWindow(window, window));
        }

        public string GenerateToken(string userKey, int verficationCodeTimeOut = 90, bool isEmail = false)
        {
            var otpSecret = Encoding.UTF8.GetBytes("the_very_very_super_secret_key_$$$$_Alwas_secret_" + userKey.Trim() + (isEmail ? "EE" : ""));
            var otp = new OtpNet.Totp(otpSecret, verficationCodeTimeOut);
            var code = otp.ComputeTotp(DateTime.Now);
            return code;
        }
    }

    public class BlockOptions
    {
        public int NumberOfRetriestOnHardBlock { get; set; }
        public int NumberOfRetriestOnSoftBlock { get; set; }
        public int NumberOfMinsInSoftBlock { get; set; }
    }
}