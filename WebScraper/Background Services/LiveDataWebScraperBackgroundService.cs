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

namespace solarmhc.Models.Background_Services
{
    public class LiveDataWebScraperBackgroundService : BackgroundService
    {
        // Inject the logger and the service provider
        private readonly ILogger _logger;
        private readonly SolarDataService _solarDataService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChromeDriverService _chromeDriverService;
        private readonly WebScraperHelperService _webScraperHelper;

        public LiveDataWebScraperBackgroundService(ILogger<LiveDataWebScraperBackgroundService> logger, IServiceProvider serviceProvider, SolarDataService solarDataService, ChromeDriverService chromeDriverService, WebScraperHelperService webScraperHelper)
        {
            // Inject the logger and the service provider
            _logger = logger;
            _serviceProvider = serviceProvider;
            _solarDataService = solarDataService;
            _chromeDriverService = chromeDriverService;
            _webScraperHelper = webScraperHelper;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Concurrent scraper service is starting.");

            stoppingToken.Register(() => _logger.LogInformation("Concurrent scraper service is stopping."));

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Concurrent scraper service is running.");

                    var tasks = new List<Task>
                    {
                       //FetchAndServeDataAsync(Constants.DataUrls.SolarEdge, stoppingToken),
                       //FetchAndServeDataAsync(Constants.DataUrls.Sunny, stoppingToken),
                       //FetchAndServeDataAsync(Constants.DataUrls.APS, stoppingToken),
                       //FetchAndServeDataAsync(Constants.DataUrls.Huawei, stoppingToken),
                        FetchAndServeDataAsync(Constants.DataUrls.Fronius, stoppingToken)
                    };

                    await Task.WhenAll(tasks);
                }
            }, stoppingToken);
        }

        private async Task FetchAndServeDataAsync(string dataUrl, CancellationToken cts)
        {
            try
            {
                // Instantiate the DataWebScraper class
                var dataWebScraper = new LiveDataWebScraper(_logger, _chromeDriverService);

                // Instantiating the service
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Start fetching the power data
                    switch (dataUrl)
                    {
                        case Constants.DataUrls.SolarEdge:
                            dataWebScraper.SolarEdgeStartFetchingPowerDataAsync(dataUrl, cts);
                            break;
                        case Constants.DataUrls.Sunny:
                            dataWebScraper.SunnyStartFetchingPowerDataAsync(dataUrl, cts);
                            break;
                        case Constants.DataUrls.APS:
                            dataWebScraper.APSStartFetchingPowerDataAsync(dataUrl, cts);
                            break;
                        case Constants.DataUrls.Huawei:
                            dataWebScraper.HuaweiStartFetchingPowerDataAsync(dataUrl, cts);
                            break;
                        case Constants.DataUrls.Fronius:
                            dataWebScraper.FroniusStartFetchingPowerDataAsync(dataUrl, cts);
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
