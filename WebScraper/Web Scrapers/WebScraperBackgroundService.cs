using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using solarmhc.Models;
using Microsoft.EntityFrameworkCore;
using WebScraper.Data;
using WebScraper;
using OpenQA.Selenium.DevTools.V123.Debugger;

namespace solarmhc.Models.Web_Scrapers
{
    internal class WebScraperBackgroundService : BackgroundService
    {
        // Inject the logger and the service provider
        private readonly ILogger<WebScraperBackgroundService> _logger;
        private readonly SolarDataService _solarDataService;
        private readonly IServiceProvider _serviceProvider;

        // Set the interval in minutes
        private readonly int _intervalInMinutes = 3;

        public WebScraperBackgroundService(ILogger<WebScraperBackgroundService> logger, IServiceProvider serviceProvider, SolarDataService solarDataService)
        {
            // Inject the logger and the service provider
            _logger = logger;
            _serviceProvider = serviceProvider;
            _solarDataService = solarDataService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Log the service running
                _logger.LogInformation("Web scraping service running at: {time}", DateTimeOffset.Now);

                // Fetch and serve the data
                await FetchAndServeData(Constants.DataUrls.SolarEdge);
                await FetchAndServeData(Constants.DataUrls.Sunny);
                await FetchAndServeData(Constants.DataUrls.APS);
                await FetchAndServeData(Constants.DataUrls.Huawei);
                await FetchAndServeData(Constants.DataUrls.Fronius);

                // Wait for the interval
                await Task.Delay(TimeSpan.FromMinutes(_intervalInMinutes), stoppingToken);
            }
        }

        private async Task FetchAndServeData(string dataUrl)
        {
            try
            {
                // Instantiate the CancellationToken
                CancellationToken cts = new CancellationToken();

                // Instantiate the DataWebScraper class
                var dataWebScraper = new DataWebScraper();

                // Instantiating the service
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Instantiate the DbContext with the scope var
                    var dbContext = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();

                    // Start fetching the power data
                    switch (dataUrl)
                    {
                        case Constants.DataUrls.SolarEdge:
                            await dataWebScraper.SolarEdgeStartFetchingPowerData(dataUrl, cts, UpdateData);
                            break;
                        case Constants.DataUrls.Sunny:
                            await dataWebScraper.SunnyStartFetchingPowerData(dataUrl, cts, UpdateData);
                            break;
                        case Constants.DataUrls.APS:
                            await dataWebScraper.APSStartFetchingPowerData(dataUrl, cts, UpdateData);
                            break;
                        case Constants.DataUrls.Huawei:
                            await dataWebScraper.HuaweiStartFetchingPowerData(dataUrl, cts, UpdateData);
                            break;
                        case Constants.DataUrls.Fronius:
                            await dataWebScraper.FroniusStartFetchingPowerData(dataUrl, cts, UpdateData);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data.");
            }
        }

        private void UpdateData(string data, string dataUrl)
        {
            // Update the data
            if (TryParseData(data, out double utilizedPower))
            {
                // Calculate the total capacity
                double totalCapacity = 0;

                // Set the total capacity based on the data URL
                switch (dataUrl)
                {
                    case Constants.DataUrls.SolarEdge:
                        totalCapacity = Constants.Capacities.SolarEdge;
                        break;
                    case Constants.DataUrls.Sunny:
                        totalCapacity = Constants.Capacities.Sunny;
                        break;
                    case Constants.DataUrls.APS:
                        totalCapacity = Constants.Capacities.APS;
                        break;
                    case Constants.DataUrls.Huawei:
                        totalCapacity = Constants.Capacities.Huawei;
                        break;
                    case Constants.DataUrls.Fronius:
                        totalCapacity = Constants.Capacities.Fronius;
                        break;
                    default:
                        totalCapacity = 25;
                        break;
                }

                // Calculate the remaining capacity and utilization percentage
                double remainingCapacity = totalCapacity - utilizedPower;
                double utilizationPercentage = (utilizedPower / totalCapacity) * 100;

                _solarDataService.UpdateData(data, new double[] { utilizedPower, remainingCapacity }, utilizationPercentage);
            }
        }

        private bool TryParseData(string data, out double utilizedPower)
        {
            utilizedPower = 0;
            // Try to parse the data by splitting the data variable, and putting the KW and util percentage in their own vars.
            try
            {
                var lines = data.Split('\n');
                if (lines.Length >= 1)
                {
                    var powerString = lines[0].Replace("KW", "").Trim();
                    if (double.TryParse(lines[1], out utilizedPower))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while parsing the data.");
            }
            return false;
        }
    }
}
