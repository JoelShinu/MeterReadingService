using System.Globalization;
using api.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        private readonly IWebHostEnvironment _env;
        public ApplicationDBContext(DbContextOptions dbContextOptions, IWebHostEnvironment env) : base(dbContextOptions)
        {
            _env = env;
        }

        public DbSet<Account> Accounts { get; set;}
        public DbSet<MeterReading> MeterReadings { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MeterReading>()
                .HasKey(mr => new { mr.AccountId, mr.MeterReadingDateTime });
        }

        public void SeedAccounts()
        {
            var path = Path.Combine(_env.ContentRootPath, "Files", "Test_Accounts.csv");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<Account>().ToList();

            if (records.Any())
            {
                using (var transaction = Database.BeginTransaction())
                {
                    Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Accounts ON");
                    foreach (var record in records)
                    {
                        if (!Accounts.Any(a => a.AccountId == record.AccountId))
                        {
                            Accounts.Add(record);
                        }
                    }
                    SaveChanges();
                    Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Accounts OFF");
                    transaction.Commit();
                }
            }
        }
    }
}