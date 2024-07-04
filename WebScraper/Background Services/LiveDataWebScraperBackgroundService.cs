using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using solarmhc.Models.Services;
using solarmhc.Models.Services.Web_Scrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper;

namespace solarmhc.Models.Background_Services
{
    public class LiveDataWebScraperBackgroundService : BackgroundService
    {
        // Inject the services
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WebScraperHelperService _webScraperHelper;
        private readonly LiveDataService _liveDataService;
        private readonly EmissionSaved _emissionSaved;
        private int intervalInMinutes = 5;

        public LiveDataWebScraperBackgroundService(ILogger<LiveDataWebScraperBackgroundService> logger, IServiceProvider serviceProvider, WebScraperHelperService webScraperHelper, LiveDataService liveDataService, EmissionSaved emissionSaved)
        {
            // Inject the services
            _logger = logger;
            _serviceProvider = serviceProvider;
            _webScraperHelper = webScraperHelper;
            _liveDataService = liveDataService;
            _emissionSaved = emissionSaved;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting web scraper services.");

            stoppingToken.Register(() => _logger.LogInformation("Stopping web scraper services."));

            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await FetchAndServeDataAsync(Constants.DataUrls.Fronius, stoppingToken, EScraper.Live);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //await _emissionSaved.EmissionCalculation(Constants.Names.SolarEdge, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees);
                    //await _emissionSaved.EmissionCalculation(Constants.Names.Sunny, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees);
                    //await _emissionSaved.EmissionCalculation(Constants.Names.APS, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees);
                    //await _emissionSaved.EmissionCalculation(Constants.Names.Huawei, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees);
                    await _emissionSaved.EmissionCalculation(Constants.Names.Fronius, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees);
                    await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Database filler service ran at: {time}", DateTimeOffset.Now);

                await FetchAndServeDataAsync(Constants.DataUrls.Fronius, null, EScraper.Data);

                await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken);
            }

            _logger.LogInformation("Web scraper services have stopped.");
        }

        private async Task FetchAndServeDataAsync(string dataUrl, CancellationToken? cts, EScraper eScraper)
        {
            try
            {
                // Instantiate the DataWebScraper class
                var dataWebScraper = new DataWebScraper(_serviceProvider, _webScraperHelper);

                // Instantiate the DataWebScraper class
                var liveWebScraper = new LiveDataWebScraper(_logger, _webScraperHelper, _serviceProvider, _liveDataService);

                if (eScraper == EScraper.Live)
                {
                    switch (dataUrl)
                    {
                        case Constants.DataUrls.SolarEdge:
                            await liveWebScraper.SolarEdgeStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Sunny:
                            await liveWebScraper.SunnyStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.APS:
                            await liveWebScraper.APSStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Huawei:
                            await liveWebScraper.HuaweiStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Fronius:
                            await liveWebScraper.FroniusStartFetchingPowerDataAsync(dataUrl);
                            break;
                        default:
                            break;
                    }
                }
                else if (eScraper == EScraper.Data)
                {
                    switch (dataUrl)
                    {
                        case Constants.DataUrls.SolarEdge:
                            await dataWebScraper.SolarEdgeStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Sunny:
                            await dataWebScraper.SunnyStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.APS:
                            await dataWebScraper.APSStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Huawei:
                            await dataWebScraper.HuaweiStartFetchingPowerDataAsync(dataUrl);
                            break;
                        case Constants.DataUrls.Fronius:
                            await dataWebScraper.FroniusStartFetchingPowerDataAsync(dataUrl);
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
    }
}
