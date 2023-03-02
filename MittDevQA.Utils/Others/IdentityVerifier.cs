using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Utils.Localizer;
using Utils.Logging.InternalLog;
using Utils.Others.Blocker;
using Utils.Senders.Email;
using Utils.Senders.Sms;

namespace Utils.Others
{
    public static class IdentityVerifierExtenstion
    {
        public static IServiceCollection AddIdentityVerifier(this IServiceCollection serviceDescriptors)
        {
            IConfiguration config = null;
            using (var scope = serviceDescriptors.BuildServiceProvider())
            {
                config = scope.GetService<IConfiguration>();
                serviceDescriptors.Configure<IdentityVerificationOptions>(config.GetSection("UserBlockOptions"));
            }

            serviceDescriptors.TryAddTransient<IdentityVerifier>();
            return serviceDescriptors;
        }
    }

    public class IdentityVerifier
    {
        private readonly SmsSender _smsService;
        private readonly UserBlocker _userBlocker;
        private readonly LocalizerService _localizerService;
        private readonly EmailService _emailSender;
        private readonly IOptions<IdentityVerificationOptions> _verfiicationOptions;
        private readonly SemiTransactionLoggger _semiLogger;

        public IdentityVerifier(SmsSender smsSender, UserBlocker userBlocker, LocalizerService localizerService,
            EmailService emailSender, IOptions<IdentityVerificationOptions> options, SemiTransactionLoggger semiLogger = null)
        {
            _smsService = smsSender;
            _userBlocker = userBlocker;
            _localizerService = localizerService;
            _emailSender = emailSender;
            _verfiicationOptions = options;
            _semiLogger = semiLogger;
        }

        private bool IsDemoAccount(string userId) => _verfiicationOptions.Value.DemoAccount == userId.Replace("A3MAL", "");

        public async Task<string> SendActiveCode(string verficationIdentity, string accountIdentity,
            IdentityType identityType, string message, string senderSpecificIdentifier = "0", int serviceId = 1, int userId = 1, bool specialMessage = false, bool transCode = false, params object[] param)
        {
            var verCode = IsDemoAccount(accountIdentity) ? "123456" : _userBlocker.GenerateToken(accountIdentity, identityType == IdentityType.Sms ?
                _verfiicationOptions.Value.SmsVerficationTimeOutInSec : _verfiicationOptions.Value.EmailVerficationTimeOutInSec, identityType == IdentityType.Email);

            var fullMessage = specialMessage
                ? _localizerService.LocalizedMessage(message, param)
                : transCode
                ? _localizerService.LocalizedMessage(message, new object[] { verCode })
                : _localizerService.LocalizedMessage(message, param) + " " + verCode;
            switch (identityType)
            {
                case IdentityType.Sms:
                    await _smsService.SendSms(verficationIdentity, fullMessage, branchId: int.Parse(senderSpecificIdentifier), serviceId: serviceId, userId: userId);
                    break;

                case IdentityType.Email:
                    _emailSender.SendEmail(new EmailContent
                    {
                        ToEmail = verficationIdentity,
                        Content = fullMessage,
                        Subject = _localizerService.LocalizedMessage("رمز تفعيل بريد الكتروني "),
                        Identifier = senderSpecificIdentifier
                    });
                    break;
            }
            if (!specialMessage)
                _semiLogger.Log(
                                new { verCode }
                               , null
                               , "Sent Active Code Via" + identityType.ToString() + " With VerCode =" + verCode);

            return verCode;
        }

        public async Task<string> IsCorrectVerCode(string identity, string vercode,
            bool withBlockProcedure = true,
            IdentityType identityType = IdentityType.Sms, string otplock = "")
        {
            if (IsDemoAccount(identity)) return null;

            if (_userBlocker.VerifyOtpLock(otplock, _verfiicationOptions.Value.SmsVerficationTimeOutInSec, _verfiicationOptions.Value.VerificationWindow))
                return "رمز_التفعيل_غير_صحيح_او_منتهي_الصلاحية";
            var correct = await _userBlocker.Verify(identity, vercode, withBlockProcedure,
             identityType == IdentityType.Sms ? _verfiicationOptions.Value.SmsVerficationTimeOutInSec :
            _verfiicationOptions.Value.EmailVerficationTimeOutInSec, _verfiicationOptions.Value.VerificationWindow, identityType == IdentityType.Email);

            return correct switch
            {
                VerificationResult.Correct => null,
                VerificationResult.IncorrectOrExpired => "رمز_التفعيل_غير_صحيح_او_منتهي_الصلاحية",
                _ => "تم_ايقاف_هوية_حسابك"
            };
        }

        public enum IdentityType
        {
            Sms,
            Email
        }
    }

    public class IdentityVerificationOptions
    {
        public int SmsVerficationTimeOutInSec { get; set; } = 60;
        public int EmailVerficationTimeOutInSec { get; set; } = 120;

        public int VerificationWindow { get; set; }
        public string DemoAccount { get; set; }
    }
}