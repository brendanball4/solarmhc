using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    public class PowerIntake
    {
        public int Id { get; set; }
        public SolarSegment ArrayName { get; set; }
        public decimal Watts { get; set; }
        public int Utilization { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal KW => Watts / 1000;
    }
}
