using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using solarmhc.Models.Data;
using solarmhc.Models.Models;
using solarmhc.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Background_Services
{
    public class DataBackgroundService : BackgroundService
    {
        // Inject the services
        private readonly ILogger<DataBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmissionSaved _emissionSaved;
        private readonly LiveDataService _liveDataService;

        public DataBackgroundService(ILogger<DataBackgroundService> logger, IServiceProvider serviceProvider, EmissionSaved emissionSaved, LiveDataService liveDataService)
        {
            // Inject the services
            _logger = logger;
            _serviceProvider = serviceProvider;
            _emissionSaved = emissionSaved;
            _liveDataService = liveDataService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting services.");
            stoppingToken.Register(() => _logger.LogInformation("Stopping services."));

            // Calculate the emissions and trees planted for each dashboard
            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>
                    {
                        _emissionSaved.EmissionCalculation(Constants.Names.SolarEdge, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                        _emissionSaved.EmissionCalculation(Constants.Names.APS, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                        _emissionSaved.EmissionCalculation(Constants.Names.Sunny, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                        _emissionSaved.EmissionCalculation(Constants.Names.Huawei, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                        _emissionSaved.EmissionCalculation(Constants.Names.Fronius, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>
                    {
                        FetchAndServeGraphDataAsync(Constants.Names.SolarEdge),
                        FetchAndServeGraphDataAsync(Constants.Names.APS),
                        FetchAndServeGraphDataAsync(Constants.Names.Sunny),
                        FetchAndServeGraphDataAsync(Constants.Names.Huawei),
                        FetchAndServeGraphDataAsync(Constants.Names.Fronius)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            // Live data for viewing on the website, currently runs every 30 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>
                {
                    FetchAndServeDataAsync(Constants.Names.SolarEdge),
                    FetchAndServeDataAsync(Constants.Names.APS),
                    FetchAndServeDataAsync(Constants.Names.Sunny),
                    FetchAndServeDataAsync(Constants.Names.Huawei),
                    FetchAndServeDataAsync(Constants.Names.Fronius)
                };

                await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Adjust the delay as needed
            }

            _logger.LogInformation("Services have stopped.");
        }

        private async Task FetchAndServeDataAsync(string dashboardId)
        {
            // Grab the database values from the given dashboardId
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();

                SolarSegment seg = await context.SolarSegments.Where(x => x.Name == dashboardId).FirstOrDefaultAsync();
                var data = await context.PowerIntakes
                    .Where(x => x.SolarSegmentId == seg.Id)
                    .OrderByDescending(x => x.TimeStamp)
                    .FirstOrDefaultAsync();

                TimeZoneInfo mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                data.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(data.TimeStamp, mountainTimeZone);

                if (data != null)
                {
                    await _liveDataService.UpdateCurrentPowerAsync(dashboardId, data.KW, data.Status);
                }
            }
        }

        private async Task FetchAndServeGraphDataAsync(string dashboardId)
        {
            // Grab the database values from the given dashboardId
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();

                SolarSegment seg = await context.SolarSegments.Where(x => x.Name == dashboardId).FirstOrDefaultAsync();
                DateTime today = DateTime.UtcNow.Date;
                List<PowerData> pData = await context.PowerIntakes
                    .Where(x => x.SolarSegmentId == seg.Id)
                    .Where(x => x.TimeStamp >= today && x.TimeStamp < today.AddDays(1))
                    .Select(x => new PowerData
                    {
                        Intake = x.KW,
                        Date = x.TimeStamp
                    })
                    .ToListAsync();

                foreach (var item in pData)
                {
                    TimeZoneInfo mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    item.Date = TimeZoneInfo.ConvertTimeFromUtc(item.Date, mountainTimeZone);
                }

                if (pData != null && pData.Count > 0)
                {
                    await _liveDataService.UpdatePowerDataAsync(dashboardId, pData);
                }
                else
                {
                    _logger.LogError($"{dashboardId}: No data found in power intakes table");
                    return;
                }
            }
        }
    }
}
