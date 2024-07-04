using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class EmissionSaved
    {
        private readonly LiveDataService _liveDataService;
        private readonly EmissionCalculator _emissionCalculator; 
        public EmissionSaved(LiveDataService liveDataService, EmissionCalculator emissionCalculator)
        {
            _liveDataService = liveDataService;
            _emissionCalculator = emissionCalculator;
        }
        public async Task EmissionCalculation(string dashboardId, decimal co2Factor, decimal trees)
        {
            await _liveDataService.UpdateCO2Async(dashboardId, _emissionCalculator.CalculateEnvironmentalImpact(dashboardId, co2Factor));
            await _liveDataService.UpdateTreesAsync(dashboardId, _emissionCalculator.CalculateEnvironmentalImpact(dashboardId, trees));
        }
    }
}
