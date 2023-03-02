using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmsSoapService;
using Utils.Others;

namespace Utils.Senders.Sms
{
    public class SmsSender
    {
        private readonly SMSSenderOption _options;
        private readonly ServiceSoapClient _smsClient;
        private readonly SMSDbContext _smsDbContext;

        public SmsSender(IOptions<SMSSenderOption> options, SMSDbContext smsDbContext = null)
        {
            _options = options.Value;
            if (options.Value.Link == "FACK") return;
            if (_options.Type == 0) _smsClient = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap12, options.Value.Link);
            _smsDbContext = smsDbContext;
        }

        public async Task SendSms(string phoneNum, string title, int branchId = 1, long smsSeqNo = -1, int serviceId = 1, int userId = 0, SmsState smsState = SmsState.New)
        {
            if (_options.Link == "FACK" && _options.Type != 1) return;

            smsSeqNo = smsSeqNo == -1 ? Guid.NewGuid().GetHashCode() : smsSeqNo;
            phoneNum = phoneNum.StartsWith("+") ? phoneNum.Trim() : ("+" + phoneNum).Trim();

            if (_options.Type == 1)
            {
                await _smsDbContext.SendSms(smsSeqNo, phoneNum, _options.BankId, branchId, title, getPriorityOfService(serviceId), serviceId,
                    userId, smsState, _options.UpdateTable);
            }
            else if (_options.Type == 0 && smsState == SmsState.New)
            {
                await _smsClient.InsertNewSMS3Async(phoneNum, title, (int)smsSeqNo, _options.BankId, branchId,
                   "OK", getPriorityOfService(serviceId), serviceId);
                //await _smsClient.InsertNewSMSAsync(phoneNum, title, (int)smsSeqNo, _options.BankId, branchId, "OK", 1);
            }
            else
                throw new AppException("Sms operation not supported");
        }

        public async Task<List<WaitingToProcessSMS>> GetUnProcessedSms()
        => await _smsDbContext.WaitingToProcessSMS.AsNoTracking().ToListAsync();

        private int getPriorityOfService(int serviceId)
        {
            var priorities = _options.Priority.Split('_').ToList();

            var matchedPriority = priorities.FirstOrDefault(x => x.Contains($"{serviceId}:"));
            if (matchedPriority == null) return 1;
            try
            {
                return int.Parse(matchedPriority.Split(':')[1]);
            }
            catch
            {
                return 1;
            }
        }
    }

    public class SMSSenderOption
    {
        public string Link { get; set; }
        public int BankId { get; set; }
        public int UpdateTable { get; set; }
        public int Type { get; set; }
        public string Priority { get; set; } = "";
        public string ArrivalView { get; set; } = null;
        public string CrDrMap { get; set; }
        public bool ValidateWithUcd { get; set; }
        public int ServiceId { get; set; }
    }

    public static class SmsSenderExtenstion
    {
        public static IServiceCollection AddSmsSender(this IServiceCollection services, bool transient = false)
        {
            IConfiguration config = null;

            using (var serviceProvider = services.BuildServiceProvider())
                config = serviceProvider.GetService<IConfiguration>();

            services.Configure<SMSSenderOption>(config.GetSection("SMSSenderOption"));
            services.AddTransient<SmsSender>();

            var type = config.GetValue<string>("SMSSenderOption:Type");
            if (!string.IsNullOrEmpty(type) && type == "1")
            {
                if (!transient)
                {
                    services.AddDbContext<SMSDbContext>(options =>
                                        options.UseSqlServer(config.GetValue<string>("SMSSenderOption:Link")));
                }
                else
                {
                    services.AddDbContext<SMSDbContext>(options =>
                    {
                        options.UseSqlServer(config.GetValue<string>("SMSSenderOption:Link"));
                    },

                            ServiceLifetime.Transient, ServiceLifetime.Transient);
                }
            }
            return services;
        }
    }

    public class SMSDbContext : DbContext
    {
        private readonly IOptions<SMSSenderOption> _senderOptions;

        public SMSDbContext(DbContextOptions<SMSDbContext> options, IOptions<SMSSenderOption> senderOptions = null)
            : base(options)
        {
            _senderOptions = senderOptions;
        }

        public DbSet<WaitingToProcessSMS> WaitingToProcessSMS { get; set; }
        public DbSet<ValidateSMS> ValidateSMS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WaitingToProcessSMS>().HasNoKey().ToView(_senderOptions?.Value?.ArrivalView ?? "WaitingToProcessSMS");
            modelBuilder.Entity<ValidateSMS>().HasKey(entity => entity.id);
            modelBuilder.HasSequence<int>("Sequence-ValidateSMS", schema: "dbo")
                  .StartsAt(1)
                  .IncrementsBy(1);
        }

        public async Task<int> GenerateSequance()
        {
            var result = new Microsoft.Data.SqlClient.SqlParameter("@result", System.Data.SqlDbType.Int);
            result.Direction = System.Data.ParameterDirection.Output;
            await Database.ExecuteSqlInterpolatedAsync($"SELECT {result} = (NEXT VALUE FOR dbo.[Sequence-ValidateSMS])");

            return (int)result.Value;
        }

        public async Task SendSms(long smsSeqNo, string phoenNumber = "0", int bankId = 1, int branchId = 1, string message = "", int priority = 1, int serviceId = 1, int userId = 0, SmsState state = SmsState.New, int updateTable = 0)
        {
            if (state == SmsState.Update)
            {
                var result = updateTable == 0 ? await Database.ExecuteSqlInterpolatedAsync($"update ValidateSMS set state =1 where SeqNo = {smsSeqNo} ") :
                       await Database.ExecuteSqlInterpolatedAsync($"update ArrivalTrans set state =1 where SeqNo = {smsSeqNo} ");
            }
            else
            {
                await Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Readysms (SMSText,MOBILENO,SEQNO,BANKID,BRANCHID,[priority] , SC , UserID) VALUES({message}, {phoenNumber}, {smsSeqNo},{bankId}, {branchId}, {priority}, {serviceId} , {userId})");
            }
        }

        //      @MESSAGE NVARCHAR(300) =null,
        //@MOBILENO NVARCHAR(15) = null,
        //@SEQNO bigint,
        //@BANKID INT =1,
        //@BRANCHID INT = 1,
        //@JustUpdateState bit =0
    }

    public class ValidateSMS
    {
        public int id { get; set; }
        public int SeqNo { get; set; }
        public string mobileno { get; set; }
        public string SplitSMS { get; set; }
        public int ServiceType { get; set; }
        public int ServiceID { get; set; }
        public int State { get; set; }
        public DateTime DATE_TIME { get; set; }
        public int BANKID { get; set; }
        public int BRANCHID { get; set; }
        public int OldBranchID { get; set; }
    }

    public class WaitingToProcessSMS
    {
        public int SmsSeqNo { get; set; }
        public string PhoneNo { get; set; }
        public string Message { get; set; }
        public int BankId { get; set; }
    }

    public enum SmsState
    {
        New = 0,
        Update = 1
    }
}