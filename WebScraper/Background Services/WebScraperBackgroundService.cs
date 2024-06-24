//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using solarmhc.Models.Services;

//namespace solarmhc.Models.Background_Services
//{
//    public class WebScraperBackgroundService : BackgroundService
//    {
//        // Inject the logger and the service provider
//        private readonly ILogger<WebScraperBackgroundService> _logger;
//        private readonly SolarDataService _solarDataService;
//        private readonly IServiceProvider _serviceProvider;
//        private readonly WebScraperHelperService _webScraperHelper;

//        // Set the interval in minutes
//        private readonly int _intervalInMinutes = 5;

//        public WebScraperBackgroundService(ILogger<WebScraperBackgroundService> logger, IServiceProvider serviceProvider, SolarDataService solarDataService, WebScraperHelperService webScraperHelper)
//        {
//            // Inject the logger and the service provider
//            _logger = logger;
//            _serviceProvider = serviceProvider;
//            _solarDataService = solarDataService;
//            _webScraperHelper = webScraperHelper;
//        }
//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                // Log the service running
//                _logger.LogInformation("Web scraping service ran at: {time}", DateTimeOffset.Now);

//                // Fetch and serve the data
//                //await FetchAndServeData(Constants.DataUrls.SolarEdge);
//                //await FetchAndServeData(Constants.DataUrls.Sunny);
//                //await FetchAndServeData(Constants.DataUrls.APS);
//                //await FetchAndServeData(Constants.DataUrls.Huawei);
//                await FetchAndServeData(Constants.DataUrls.Fronius);

//                // Wait for the interval
//                await Task.Delay(TimeSpan.FromMinutes(_intervalInMinutes), stoppingToken);
//            }
//        }

//        private async Task FetchAndServeData(string dataUrl)
//        {
//            try
//            {
//                // Instantiate the DataWebScraper class
//                var dataWebScraper = new DataWebScraper(_serviceProvider, _webScraperHelper);

//                // Instantiating the service
//                using (var scope = _serviceProvider.CreateScope())
//                {
//                    // Start fetching the power data
//                    switch (dataUrl)
//                    {
//                        case Constants.DataUrls.SolarEdge:
//                            await dataWebScraper.SolarEdgeStartFetchingPowerData(dataUrl);
//                            break;
//                        case Constants.DataUrls.Sunny:
//                            await dataWebScraper.SunnyStartFetchingPowerData(dataUrl);
//                            break;
//                        case Constants.DataUrls.APS:
//                            await dataWebScraper.APSStartFetchingPowerData(dataUrl);
//                            break;
//                        case Constants.DataUrls.Huawei:
//                            await dataWebScraper.HuaweiStartFetchingPowerData(dataUrl);
//                            break;
//                        case Constants.DataUrls.Fronius:
//                            await dataWebScraper.FroniusStartFetchingPowerData(dataUrl);
//                            break;
//                        default:
//                            break;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while fetching data.");
//            }
//        }
//    }
//}
