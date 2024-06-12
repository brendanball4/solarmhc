using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    internal class InverterType
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public decimal Capacity { get; set; } // in kW
    }
}
