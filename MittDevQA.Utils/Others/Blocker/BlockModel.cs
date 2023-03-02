using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Utils.Others.Blocker
{
    public class BlockModel
    {
        [Key]
        public string UserKey { get; set; }

        public bool IsBlocked { get; set; }
        public int NumberOFRetry { get; set; }
    }

    public partial class BlocksDBContext : DbContext
    {
        public BlocksDBContext()
        {
        }

        public BlocksDBContext(DbContextOptions<BlocksDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=10.10.20.45;Initial Catalog=managment_LAST;User ID=sa;Password=lemon2020;");
                //optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=userBlocks;User ID=sa;Password=123456");
            }

            optionsBuilder.EnableSensitiveDataLogging();
        }

        public virtual DbSet<BlockModel> UserBlocks { get; set; }
    }

    public enum VerificationResult
    {
        Correct,
        IncorrectOrExpired,
        Blocked
    }
}