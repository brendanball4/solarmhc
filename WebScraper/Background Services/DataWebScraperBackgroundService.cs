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
    public class DataWebScraperBackgroundService : BackgroundService
    {
        // Inject the services
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WebScraperHelperService _webScraperHelper;
        private readonly LiveDataService _liveDataService;
        private readonly EmissionSaved _emissionSaved;
        private int intervalInMinutes = 5;

        public DataWebScraperBackgroundService(ILogger<DataWebScraperBackgroundService> logger, IServiceProvider serviceProvider, WebScraperHelperService webScraperHelper, LiveDataService liveDataService, EmissionSaved emissionSaved)
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
                    var tasks = new List<Task>
                    {
                        //_emissionSaved.EmissionCalculation(Constants.Names.SolarEdge, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                        //_emissionSaved.EmissionCalculation(Constants.Names.APS, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                         _emissionSaved.EmissionCalculation(Constants.Names.Sunny, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                         _emissionSaved.EmissionCalculation(Constants.Names.Huawei, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees),
                         _emissionSaved.EmissionCalculation(Constants.Names.Fronius, Constants.Environmental.Canada.CO2Factor, Constants.Environmental.Canada.Trees)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>
                    {
                        //FetchAndServeDataAsync(Constants.DataUrls.SolarEdge, Constants.Names.SolarEdge, EScraper.Data),
                        //FetchAndServeDataAsync(Constants.DataUrls.APS, Constants.Names.APS, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Sunny, Constants.Names.Sunny, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Huawei, Constants.Names.Huawei, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Fronius, Constants.Names.Fronius, EScraper.Data)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>
                {
                    FetchAndServeDataAsync(Constants.DataUrls.Sunny, Constants.Names.Sunny, EScraper.Live),
                    FetchAndServeDataAsync(Constants.DataUrls.Huawei, Constants.Names.Huawei, EScraper.Live),
                    FetchAndServeDataAsync(Constants.DataUrls.Fronius, Constants.Names.Fronius, EScraper.Live)
                };

                await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Adjust the delay as needed
            }

            _logger.LogInformation("Web scraper services have stopped.");
        }


        private async Task FetchAndServeDataAsync(string dataUrl, string dashboardId, EScraper eScraper)
        {
            try
            {
                // Instantiate the DataWebScraper class
                var dataScraper = new DataWebScraper(_webScraperHelper, _serviceProvider, _liveDataService);

                var authSelectors = new AuthSelectors();
                var selectors = new ScrapingSelectors();

                switch (dataUrl)
                {
                    case Constants.DataUrls.SolarEdge:
                        //await liveWebScraper.GenericFetchPowerAsyncDataAsync();
                        break;
                    case Constants.DataUrls.Sunny:
                        authSelectors = new AuthSelectors
                        {
                            UsernameField = Constants.TargetedElements.Sunny.Auth.username,
                            PasswordField = Constants.TargetedElements.Sunny.Auth.password,
                            LoginButtonField = Constants.TargetedElements.Sunny.Auth.loginButton,
                            EnvUsername = Constants.EnvironmentVars.EnvironmentNames.Sunny,
                            EnvPassword = Constants.EnvironmentVars.EnvironmentPass.Sunny
                        };
                        selectors = new ScrapingSelectors
                        {
                            WaitConditionAuth = Constants.TargetedElements.Sunny.Auth.loginButton,
                            WaitCondition = Constants.TargetedElements.Sunny.Data.kwId,
                            PowerField = Constants.TargetedElements.Sunny.Data.kwId
                        };
                        await dataScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, authSelectors, true, false);
                        break;
                    case Constants.DataUrls.APS:
                        selectors = new ScrapingSelectors
                        {
                            WaitCondition = Constants.TargetedElements.APS.Data.kwId,
                            PowerField = Constants.TargetedElements.APS.Data.kwId
                        };
                        await dataScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, null, false, false);
                        break;
                    case Constants.DataUrls.Huawei:
                        authSelectors = new AuthSelectors
                        {
                            UsernameField = Constants.TargetedElements.Huawei.Auth.username,
                            PasswordField = Constants.TargetedElements.Huawei.Auth.password,
                            LoginButtonField = Constants.TargetedElements.Huawei.Auth.loginButton,
                            EnvUsername = Constants.EnvironmentVars.EnvironmentNames.Huawei,
                            EnvPassword = Constants.EnvironmentVars.EnvironmentPass.Huawei
                        };
                        selectors = new ScrapingSelectors
                        {
                            WaitConditionAuth = Constants.TargetedElements.Huawei.Auth.loginButton,
                            WaitCondition = Constants.TargetedElements.Huawei.Data.kwId,
                            PowerField = Constants.TargetedElements.Huawei.Data.kwId
                        };
                        await dataScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, authSelectors, false, true);
                        break;
                    case Constants.DataUrls.Fronius:
                        selectors = new ScrapingSelectors
                        {
                            WaitCondition = Constants.TargetedElements.Fronius.Data.kwId,
                            PowerField = Constants.TargetedElements.Fronius.Data.kwId
                        };
                        await dataScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, null, false, false);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data.");
            }
        }
    }
}
