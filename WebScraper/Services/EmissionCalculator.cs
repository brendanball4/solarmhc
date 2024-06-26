using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.DevTools.V123.Animation;
using OpenQA.Selenium.DevTools.V123.Runtime;
using OpenQA.Selenium.Internal.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Data;
using WebScraper.Models;

namespace solarmhc.Models.Services
{
    public class EmissionCalculator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmissionCalculator> _logger;
        public EmissionCalculator(ILogger<EmissionCalculator> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

        }
        private double EnergyCalculation()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                double totalEnergy = 0;
                var dbContext = scope.ServiceProvider.GetService<SolarMHCDbContext>();
                List<PowerIntake> readings = dbContext.PowerIntakes.ToListAsync().Result;

                for (int i = 1; i < readings.Count; i++)
                {
                    var prev = readings[i - 1];
                    var current = readings[i];
                    var timeDifference = (current.TimeStamp - prev.TimeStamp).TotalHours;
                    var averagePower = (prev.KW + current.KW) / 2;
                    totalEnergy += (double)averagePower * timeDifference;
                }

                return totalEnergy;
            }
        }

        public double CalculateEnvironmentalImpact(decimal factor)
        {
            var totalEnergy = EnergyCalculation();

            if (factor == Constants.Environmental.Canada.CO2Factor)
            {
                var emissions = totalEnergy * ((double)factor / 1000);
                _logger.LogInformation($"Calculating CO2 Emissions saved. ({emissions} kg of CO2 saved)");
                return emissions;
            }
            else if (factor == Constants.Environmental.Canada.Trees)
            {
                var trees = totalEnergy * (double)factor;
                _logger.LogInformation($"Calculating Equivalent Trees planted. ({trees} trees planted)");
                return trees;
            }

            _logger.LogError($"Factor: ({factor}) not equal to Trees or CO2 Factor");
            return 0;
        }
    }
}
