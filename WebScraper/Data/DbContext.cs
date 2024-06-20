using Microsoft.EntityFrameworkCore;
using solarmhc.Models.Data;
using WebScraper.Models;

namespace WebScraper.Data
{
    public class SolarMHCDbContext : DbContext
    {
        public SolarMHCDbContext(DbContextOptions<SolarMHCDbContext> options) : base(options) { }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<PowerIntake> PowerIntakes { get; set; }
        public DbSet<SolarSegment> SolarSegments { get; set; }
        public DbSet<InverterType> InverterTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships
            modelBuilder.Entity<Model>()
                .HasOne(m => m.Brand)
                .WithMany()
                .HasForeignKey(m => m.BrandId);

            modelBuilder.Entity<InverterType>()
                .HasOne(it => it.Model)
                .WithMany()
                .HasForeignKey(it => it.ModelId);

            modelBuilder.Entity<SolarSegment>()
                .HasOne(ss => ss.InverterType)
                .WithMany()
                .HasForeignKey(ss => ss.InverterTypeId);

            modelBuilder.Entity<SolarSegment>()
                .HasOne(ss => ss.Model)
                .WithMany()
                .HasForeignKey(ss => ss.ModelId);

            modelBuilder.Entity<PowerIntake>()
                .HasOne(pi => pi.SolarSegment)
                .WithMany()
                .HasForeignKey(pi => pi.SolarSegmentId);

            // Precision settings
            modelBuilder.Entity<InverterType>()
                .Property(i => i.RatedCapacity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SolarSegment>()
                .Property(s => s.RatedCapacity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PowerIntake>()
                .Property(p => p.KW)
                .HasPrecision(18, 2);

            Seed.Seeder(modelBuilder);
        }
    }
}
