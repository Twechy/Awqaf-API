using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Db;

public class AwqafDb : DbContext
{
    public AwqafDb()
    {
    }

    public AwqafDb(DbContextOptions<AwqafDb> dbContextOptions) : base(dbContextOptions)
    {
    }

    public DbSet<Guardian> Guardians { get; set; }
    public DbSet<Mosque> Mosques { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<Memorization> Memorizations { get; set; }
    public DbSet<MemorizationRecord> MemorizationRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer($"Data Source=.;Initial Catalog={nameof(AwqafDb)};Trusted_Connection=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Memorization>()
            .HasOne(x => x.Student)
            .WithOne(x => x.Memorization)
            .HasForeignKey<Memorization>(ad => ad.StudentId);

        modelBuilder.Entity<Mosque>()
            .Property(x => x.Location)
            .HasConversion(s => JsonConvert.SerializeObject(s), m => JsonConvert.DeserializeObject<Location>(m));
    }
}