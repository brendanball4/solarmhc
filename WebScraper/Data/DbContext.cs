using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<InverterType>()
                .Property(i => i.RatedCapacity)
                .HasPrecision(18, 2); // Specify precision and scale

            modelBuilder.Entity<SolarSegment>()
                .Property(s => s.RatedCapacity)
                .HasPrecision(18, 2); // Specify precision and scale

            modelBuilder.Entity<PowerIntake>()
                .Property(p => p.Watts)
                .HasPrecision(18, 2); // Specify precision and scale
        }
    }
}
