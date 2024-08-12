using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class Graphing
    {
        private readonly LiveDataService _liveDataService;

        public Graphing(LiveDataService liveDataService)
        {
            _liveDataService = liveDataService;
        }
        public async Task GetGraphValues(string dashboardId)
        {
            await _liveDataService.UpdatePowerDataAsync(dashboardId);
        }
    }
}
