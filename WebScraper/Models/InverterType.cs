using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Models
{
    public class InverterType
    {
        public int Id { get; set; }
        public Model Model { get; set; }
        public int ModelId { get; set; }
        public decimal RatedCapacity { get; set; }
        public ELocation Location { get; set; }
        public string? Notes { get; set; }
    }
}
