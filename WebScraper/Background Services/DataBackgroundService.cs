using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.DevTools.V123.Network;
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

            // Calculate the power intake data for graphing
            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>
                    {
                        _liveDataService.SetPowerData(Constants.Names.SolarEdge),
                        _liveDataService.SetPowerData(Constants.Names.APS),
                        _liveDataService.SetPowerData(Constants.Names.Sunny),
                        _liveDataService.SetPowerData(Constants.Names.Huawei),
                        _liveDataService.SetPowerData(Constants.Names.Fronius)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

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

            // Functions that collect data for the database, currently runs every 5 minutes
            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tasks = new List<Task>
                    {
                        // This collects data for the database and live viewing for SolarEdge
                        FetchAndServeDataAsync(Constants.DataUrls.SolarEdge, Constants.Names.SolarEdge, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.APS, Constants.Names.APS, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Sunny, Constants.Names.Sunny, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Huawei, Constants.Names.Huawei, EScraper.Data),
                        FetchAndServeDataAsync(Constants.DataUrls.Fronius, Constants.Names.Fronius, EScraper.Data)
                    };

                    await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust the delay as needed
                }
            }, stoppingToken);

            // Live data for viewing on the website, currently runs every 30 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>
                {
                    FetchAndServeDataAsync(Constants.DataUrls.APS, Constants.Names.APS, EScraper.Live),
                    FetchAndServeDataAsync(Constants.DataUrls.Sunny, Constants.Names.Sunny, EScraper.Live),
                    FetchAndServeDataAsync(Constants.DataUrls.Huawei, Constants.Names.Huawei, EScraper.Live),
                    FetchAndServeDataAsync(Constants.DataUrls.Fronius, Constants.Names.Fronius, EScraper.Live)
                };

                await Task.WhenAll(tasks); // Starts tasks concurrently and waits for all to complete
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Adjust the delay as needed
            }

            _logger.LogInformation("Services have stopped.");
        }


        private async Task FetchAndServeDataAsync(string dataUrl, string dashboardId, EScraper eScraper)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dataWebScraper = scope.ServiceProvider.GetService<DataWebScraper>();

                    if (dataWebScraper == null)
                    {
                        _logger.LogError("There was an error creating a data web scraper class scope.");
                    }

                    var authSelectors = new AuthSelectors();
                    var selectors = new ScrapingSelectors();

                    switch (dataUrl)
                    {
                        case Constants.DataUrls.SolarEdge:
                            await dataWebScraper.FetchPowerDataSolarEdgeAPI();
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
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, authSelectors, true, false);
                            break;
                        case Constants.DataUrls.APS:
                            authSelectors = new AuthSelectors
                            {
                                UsernameField = Constants.TargetedElements.APS.Auth.username,
                                PasswordField = Constants.TargetedElements.APS.Auth.password,
                                LoginButtonField = Constants.TargetedElements.APS.Auth.loginButton,
                                EnvUsername = Constants.EnvironmentVars.EnvironmentNames.APS,
                                EnvPassword = Constants.EnvironmentVars.EnvironmentPass.APS
                            };
                            selectors = new ScrapingSelectors
                            {
                                WaitConditionAuth = Constants.TargetedElements.APS.Auth.loginButton,
                                WaitCondition = Constants.TargetedElements.APS.Data.kwId,
                                PowerField = Constants.TargetedElements.APS.Data.kwId
                            };
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, authSelectors, false, false);
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
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, authSelectors, false, true);
                            break;
                        case Constants.DataUrls.Fronius:
                            selectors = new ScrapingSelectors
                            {
                                WaitCondition = Constants.TargetedElements.Fronius.Data.kwId,
                                PowerField = Constants.TargetedElements.Fronius.Data.kwId
                            };
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, selectors, eScraper, null, false, false);
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
