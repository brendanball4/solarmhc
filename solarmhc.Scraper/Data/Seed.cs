using Microsoft.EntityFrameworkCore;
using solarmhc.Scraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Data
{
    public static class Seed
    {
        public static void Seeder(ModelBuilder modelBuilder)
        {
            // Seeding for Brand model
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "LG" },
                new Brand { Id = 2, Name = "SolarEdge" },
                new Brand { Id = 3, Name = "Sunny" },
                new Brand { Id = 4, Name = "APS" },
                new Brand { Id = 5, Name = "Huawei" },
                new Brand { Id = 6, Name = "Fronius" }
            );

            // Seeding for Model model
            modelBuilder.Entity<Model>().HasData(
                new Model { Id = 1, Name = "N2T-A5", BrandId = 1 },
                new Model { Id = 2, Name = "SE20KUS", BrandId = 2 },
                new Model { Id = 3, Name = "SUNNY TRIPOWER 24000TL-US", BrandId = 3 },
                new Model { Id = 4, Name = "1000YC", BrandId = 4 },
                new Model { Id = 5, Name = "25KTL inverter", BrandId = 5 },
                new Model { Id = 6, Name = "SYMO 24.0-3-480", BrandId = 6 }
            );

            // Seeding for InverterType model
            modelBuilder.Entity<InverterType>().HasData(
                new InverterType { Id = 1, RatedCapacity = 20000, Location = ELocation.CarportCanopy, Notes = "SolarEdge", ModelId = 2 },
                new InverterType { Id = 2, RatedCapacity = 24000, Location = ELocation.CarportCanopy, Notes = "Sunny", ModelId = 3 },
                new InverterType { Id = 3, RatedCapacity = 1000, Location = ELocation.CarportCanopy, Notes = "APS", ModelId = 4 },
                new InverterType { Id = 4, RatedCapacity = 24000, Location = ELocation.CarportCanopy, Notes = "Huawei", ModelId = 5 },
                new InverterType { Id = 5, RatedCapacity = 23955, Location = ELocation.CarportCanopy, Notes = "Fronius", ModelId = 6 }
            );

            // Seeding for SolarSegment model
            modelBuilder.Entity<SolarSegment>().HasData(
                new SolarSegment { Id = 1, Name = "SolarEdge", ModelId = 1, InstallationDate = null, Location = ELocation.CarportCanopy, InverterTypeId = 1, RatedCapacity = 385, Notes = "Field Segment 3" },
                new SolarSegment { Id = 2, Name = "Sunny", ModelId = 1, InstallationDate = null, Location = ELocation.CarportCanopy, InverterTypeId = 2, RatedCapacity = 385, Notes = "Field Segment 4" },
                new SolarSegment { Id = 3, Name = "APS", ModelId = 1, InstallationDate = null, Location = ELocation.CarportCanopy, InverterTypeId = 3, RatedCapacity = 385, Notes = "Field Segment 2" },
                new SolarSegment { Id = 4, Name = "Huawei", ModelId = 1, InstallationDate = null, Location = ELocation.CarportCanopy, InverterTypeId = 4, RatedCapacity = 385, Notes = "Field Segment 1" },
                new SolarSegment { Id = 5, Name = "Fronius", ModelId = 1, InstallationDate = null, Location = ELocation.CarportCanopy, InverterTypeId = 5, RatedCapacity = 385, Notes = "Field Segment 5" }
            );
        }
    }
}
