using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Models
{
    public class SolarSegment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Model Model { get; set; }
        public int ModelId { get; set; }
        public DateTime? InstallationDate { get; set; }
        public ELocation Location { get; set; }   
        public InverterType InverterType { get; set; }
        public int InverterTypeId { get; set; }
        public decimal RatedCapacity { get; set; }
        public string? Notes { get; set; }
    }
}
