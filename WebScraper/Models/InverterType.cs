using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    public class InverterType
    {
        public int Id { get; set; }
        public Model Model { get; set; }
        public decimal RatedCapacity { get; set; }
        public Location Location { get; set; }
        public string? Notes { get; set; }
    }
}
