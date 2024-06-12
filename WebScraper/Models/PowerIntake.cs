using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    internal class PowerIntake
    {
        public int Id { get; set; }
        public SolarArray ArrayName { get; set; }
        public decimal KW { get; set; }
        public int Utilization { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
