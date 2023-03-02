using Microsoft.EntityFrameworkCore;
using Utils.Otp.Models;

namespace Utils.Otp
{
    public class OTP_DBContext : DbContext
    {
        public OTP_DBContext(DbContextOptions<OTP_DBContext> options)
            : base(options)
        {
        }

        public DbSet<TokenUser> TokenUser { get; set; }
        public DbSet<Token_SN_Info> Token_SN_Info { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            optionsBuilder.UseSqlServer(
                //"Server=10.10.20.45;Database=OTP_DB;Trusted_Connection=true;User ID = osama; Password = ++osama2020++;");
                "Server=10.10.20.46;Database=OTP;Trusted_Connection=true;User ID = sa; Password = lemon2020;");
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<TokenUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.ActDate)
                    .HasColumnName("act_date")
                    .HasColumnType("date");

                entity.Property(e => e.Bankid).HasColumnName("bankid");

                entity.Property(e => e.Branchid).HasColumnName("branchid");

                entity.Property(e => e.DateInsert)
                    .HasColumnName("date_insert")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeActDate)
                    .HasColumnName("deAct_date")
                    .HasColumnType("date");

                entity.Property(e => e.Sn).HasColumnName("SN");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Token_SN_Info>(entity => { entity.HasKey(e => e.SN); });
        }
    }
}