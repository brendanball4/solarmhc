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
        public async Task FroniusEmissionCalculation()
        {
            double CO2 = _emissionCalculator.CalculateEnvironmentalImpact(Constants.Environmental.Canada.CO2Factor);
            double Trees = _emissionCalculator.CalculateEnvironmentalImpact(Constants.Environmental.Canada.Trees);
            await _liveDataService.UpdateCO2Async(CO2);
            await _liveDataService.UpdateTreesAsync(Trees);
        }
    }
}
