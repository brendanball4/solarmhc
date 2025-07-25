﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using solarmhc.Models.Data;
using solarmhc.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private double EnergyCalculation(string dashboardId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                double totalEnergy = 0;
                var dbContext = scope.ServiceProvider.GetService<SolarMHCDbContext>();
                SolarSegment seg = dbContext.SolarSegments.FirstOrDefault(x => x.Name == dashboardId);
                List<PowerIntake> readings = dbContext.PowerIntakes.Where(pi => pi.SolarSegmentId == seg.Id).ToListAsync().Result;

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

        public double CalculateEnvironmentalImpact(string dashboardId, decimal factor)
        {
            double totalEnergy = EnergyCalculation(dashboardId);

            if (factor == Constants.Environmental.Canada.CO2Factor)
            {
                double emissions = totalEnergy * ((double)factor / 1000);

                switch (dashboardId)
                {
                    case Constants.Names.SolarEdge:
                        emissions += (double)Constants.Environmental.StartingStatistics.CO2Emissions.SolarEdge;
                        break;
                    case Constants.Names.Sunny:
                        emissions += (double)Constants.Environmental.StartingStatistics.CO2Emissions.Sunny;
                        break;
                    case Constants.Names.APS:
                        emissions += (double)Constants.Environmental.StartingStatistics.CO2Emissions.APS;
                        break;
                    case Constants.Names.Huawei:
                        emissions += (double)Constants.Environmental.StartingStatistics.CO2Emissions.Huawei;
                        break;
                    case Constants.Names.Fronius:
                        emissions += (double)Constants.Environmental.StartingStatistics.CO2Emissions.Fronius;
                        break;
                    default:
                        break;
                }

                _logger.LogInformation($"Calculating CO2 Emissions saved. ({emissions} kg of CO2 saved)");
                return emissions;
            }
            else if (factor == Constants.Environmental.Canada.Trees)
            {
                var trees = totalEnergy * (double)factor;

                switch (dashboardId)
                {
                    case Constants.Names.SolarEdge:
                        trees += (double)Constants.Environmental.StartingStatistics.TreesPlanted.SolarEdge;
                        break;
                    case Constants.Names.Sunny:
                        trees += (double)Constants.Environmental.StartingStatistics.TreesPlanted.Sunny;
                        break;
                    case Constants.Names.APS:
                        trees += (double)Constants.Environmental.StartingStatistics.TreesPlanted.APS;
                        break;
                    case Constants.Names.Huawei:
                        trees += (double)Constants.Environmental.StartingStatistics.TreesPlanted.Huawei;
                        break;
                    case Constants.Names.Fronius:
                        trees += (double)Constants.Environmental.StartingStatistics.TreesPlanted.Fronius;
                        break;
                    default:
                        break;
                }

                _logger.LogInformation($"Calculating Equivalent Trees planted. ({trees} trees planted)");
                return trees;
            }

            _logger.LogError($"Factor: ({factor}) not equal to Trees or CO2 Factor");
            return 0;
        }
    }
}
