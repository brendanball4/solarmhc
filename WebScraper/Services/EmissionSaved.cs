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
<<<<<<< HEAD
            await _liveDataService.UpdateCO2Async(dashboardId, _emissionCalculator.CalculateEnvironmentalImpact(co2Factor));
            await _liveDataService.UpdateTreesAsync(dashboardId, _emissionCalculator.CalculateEnvironmentalImpact(trees));
=======
            double CO2 = _emissionCalculator.CalculateEnvironmentalImpact(Constants.Environmental.Canada.CO2Factor);
            double Trees = _emissionCalculator.CalculateEnvironmentalImpact(Constants.Environmental.Canada.Trees);
            await _liveDataService.UpdateCO2Async(CO2);
            await _liveDataService.UpdateTreesAsync(Trees);
>>>>>>> a1e8592c38c1cb5f6f9acfb46c2ccfc3c3e5d008
        }
    }
}
