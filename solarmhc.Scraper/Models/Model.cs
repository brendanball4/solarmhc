using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Brand Brand { get; set; }
        public int BrandId { get; set; }
    }
}
