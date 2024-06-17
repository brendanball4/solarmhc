using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    public class PowerIntake
    {
        public int Id { get; set; }
        public SolarSegment ArrayName { get; set; }
        public decimal KW { get; set; }
        public double Utilization { get; set; }
        public DateTime TimeStamp { get; set; }
        [NotMapped]
        public decimal Watts => KW * 1000;
    }
}
