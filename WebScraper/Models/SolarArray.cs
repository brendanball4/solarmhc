using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Models
{
    public enum Status
    {
        Active = 1,
        Offline = 2
    }

    public enum ArrayLocation
    {
        Canopy = 1,
        Residence = 2,
        Other = 3
    }

    internal class SolarArray
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public DateTime InstallationDate { get; set; }
        public ArrayLocation Location { get; set; }
        public decimal Capacity { get; set; }
        public decimal Efficiency { get; set; }
        public Status Status { get; set; }
        public InverterType InverterType { get; set; }
        public string Notes { get; set; }
    }
}
