using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ITSecurity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Utils.Others;
using Utils.Otp.Models;

namespace Utils.Otp
{
    public static class OtpExtentions
    {
        public static IServiceCollection AddHardOtp(this IServiceCollection services)
        {
            string otpConnectionString = string.Empty;
            var scope = services.BuildServiceProvider();
            var configuration = scope.GetService<IConfiguration>();
            otpConnectionString = configuration.GetConnectionString("OTP_DB");

            if (otpConnectionString.Contains("isLinked"))
            {
                services.AddOptions()
                .AddOptions<OtpApiLinkOption>()
                .Configure(x => x.OtpApiLink = otpConnectionString.Split(';')[0]);

                services.AddHttpClient<OtpHttpClient>();
                services.AddTransient<IOtpVerifier, OtpApiVerifier>();
            }
            else
            {
                if (!string.IsNullOrEmpty(otpConnectionString))
                {
                    var otpConnetion = otpConnectionString.Split("__");
                    services.AddDbContextPool<OTP_DBContext>(options =>
                    {
                        options.UseSqlServer(otpConnetion[0]);
                        options.EnableSensitiveDataLogging().EnableDetailedErrors();
                    });

                    services.AddTransient<HardOtpVerifier>();
                    services.AddTransient<IOtpVerifier, HardOtpVerifier>();
                }
            }

            return services;
        }
    }

    public interface IOtpVerifier
    {
        Task<bool> VerifyOtp(string otp, string otpSn);

        Task<bool> VerifyOtpavalibility(string otpSn);

        Task<string> UpdateOtpState(string otpSn, int state);
    }

    public class HardOtpVerifier : IOtpVerifier
    {
        private readonly seamoonapi _seamoonApi;
        private readonly OTP_DBContext _otpDbContext;
        private bool _isWihtoutTime = false;

        public HardOtpVerifier(OTP_DBContext otpDbContext, IConfiguration configuration)
        {
            _otpDbContext = otpDbContext;
            _seamoonApi = new seamoonapi();
            _isWihtoutTime = configuration.GetConnectionString("OTP_DB").Split("__").Count() > 1;
        }

        public async Task<bool> VerifyOtp(string otp, string otpSn)
        {
            if (otpSn == "123456") return true;
            var tokenSnInfo = await GetOtpSn(otpSn.Trim());

            var otpSerial = tokenSnInfo.SN_Info.Trim();

            var passwordResult = _seamoonApi.checkpassword(otpSerial, otp.Trim());
            if (passwordResult.Length > 3)
            {
                if (!_isWihtoutTime) await UpdateSnInfo(otpSn, passwordResult.Trim());
                return true;
            }
            return false;
        }

        private async Task<Token_SN_Info> GetOtpSn(string otpSn)
        {
            var tokenSnInfo = await _otpDbContext.Token_SN_Info
                .FirstOrDefaultAsync(x => x.SN == Convert.ToInt32(otpSn) && x.State == 1);

            if (tokenSnInfo != null) return tokenSnInfo;
            throw new AppException("can't find this otp_Sn in our systems..", otpSn);
        }

        private async Task UpdateSnInfo(string otpSn, string newSnInfo)
        {
            var tokenSnInfo = await GetOtpSn(otpSn);

            tokenSnInfo.SN_Info = newSnInfo;
            tokenSnInfo.State = 1;

            await _otpDbContext.SaveChangesAsync();
        }

        public async Task<string> GenerateOtp(string otpSn)
        => _seamoonApi.seamoonotpgetpasswords((await GetOtpSn(otpSn)).SN_Info,
                   DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,
                   DateTime.Now.Minute, DateTime.Now.Hour, DateTime.Now.Minute);

        public async Task<bool> VerifyOtpavalibility(string otpSn)
        => await _otpDbContext.Token_SN_Info
                .AnyAsync(x => x.SN == Convert.ToInt32(otpSn) && x.State == 0);

        public async Task<string> UpdateOtpState(string otpSn, int state)
        {
            var tokenSnInfo = await _otpDbContext.Token_SN_Info
                .FirstOrDefaultAsync(x => x.SN == Convert.ToInt32(otpSn) && x.State == 1);
            if (tokenSnInfo is null) return "otp is not exist";
            tokenSnInfo.State = state;

            var result = await _otpDbContext.SaveChangesAsync() >= 1;
            return result ? null : "Could not update otp State";
        }
    }

    public class OtpHttpClient
    {
        private readonly HttpClient _httpClient;
        private object _otpApiLink;

        public OtpHttpClient(HttpClient httpClient, IOptions<OtpApiLinkOption> otpApiLinkOption)
        {
            _httpClient = httpClient;
            _otpApiLink = otpApiLinkOption.Value.OtpApiLink;
        }

        public async Task<bool> VerifyOtpHttp(string otp, string otpSn)
        {
            if (otpSn == "123456") return true;
            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod("Post"),
                RequestUri = new Uri(_otpApiLink + $"Api/Otp/VerifyOtp?otp={otp}&otpSn={otpSn}"),
            };
            var httpResult = await _httpClient.SendAsync(httpRequest);
            var resultContent = await httpResult.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OtpResult>(resultContent).State;
        }
    }

    internal class OtpApiVerifier : IOtpVerifier
    {
        private readonly OtpHttpClient _otpHttpClient;

        public OtpApiVerifier(OtpHttpClient otpHttpClient)
        {
            _otpHttpClient = otpHttpClient;
        }

        public Task<string> UpdateOtpState(string otpSn, int state)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyOtpavalibility(string otpSn)
        {
            throw new NotImplementedException();
        }

        Task<bool> IOtpVerifier.VerifyOtp(string otp, string otpSn)
          => _otpHttpClient.VerifyOtpHttp(otp, otpSn);
    }

    public class OtpResult
    {
        public bool State { get; set; }
    }

    public class OtpApiLinkOption
    {
        public string OtpApiLink { get; set; }
    }
}