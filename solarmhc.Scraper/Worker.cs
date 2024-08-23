using OpenQA.Selenium;
using solarmhc.Scraper.Models;
using solarmhc.Scraper.Services;
using System.Threading.Tasks;

namespace solarmhc.Scraper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChromeDriverManager _chromedDriverManager;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, ChromeDriverManager chromedDriverManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _chromedDriverManager = chromedDriverManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("SolarEdge worker running at: {time}", DateTimeOffset.Now);

                    // Create a function(s) to run on repeated tasks 
                    var tasks = new List<Task>
                    {
                        FetchDataAsync(Constants.DataUrls.LoginPages.SolarEdge, Constants.Names.SolarEdge),
                    };

                    await Task.WhenAll(tasks);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Main worker running at: {time}", DateTimeOffset.Now);

                // Create a function(s) to run on repeated tasks 
                var tasks = new List<Task>
                {
                    FetchDataAsync(Constants.DataUrls.LoginPages.APS, Constants.Names.APS),
                    FetchDataAsync(Constants.DataUrls.LoginPages.Sunny, Constants.Names.Sunny),
                    FetchDataAsync(Constants.DataUrls.LoginPages.Huawei, Constants.Names.Huawei),
                    FetchDataAsync(Constants.DataUrls.LoginPages.Fronius, Constants.Names.Fronius)
                };

                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task FetchDataAsync(string dataUrl, string dashboardId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dataWebScraper = scope.ServiceProvider.GetService<WebScraperService>();

                    if (dataWebScraper == null)
                    {
                        _logger.LogError("There was an error creating a data web scraper class scope.");
                    }

                    var authSelectors = new AuthSelectors();
                    var selectors = new ScrapingSelectors();

                    switch (dataUrl)
                    {
                        case Constants.DataUrls.LoginPages.SolarEdge:
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function started successfully.");
                            await dataWebScraper.FetchPowerDataSolarEdgeAPI();
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function ended.");
                            break;
                        case Constants.DataUrls.LoginPages.Sunny:
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
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function started successfully.");
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, Constants.DataUrls.DataPages.Sunny, selectors, authSelectors, true, false);
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function ended.");
                            break;
                        case Constants.DataUrls.LoginPages.APS:
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
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function started successfully.");
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, Constants.DataUrls.DataPages.APSData, selectors, authSelectors, false, false);
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function ended.");
                            break;
                        case Constants.DataUrls.LoginPages.Huawei:
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
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function started successfully.");
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, Constants.DataUrls.DataPages.Huawei, selectors, authSelectors, false, true);
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function ended.");
                            break;
                        case Constants.DataUrls.LoginPages.Fronius:
                            selectors = new ScrapingSelectors
                            {
                                WaitCondition = Constants.TargetedElements.Fronius.Data.kwId,
                                PowerField = Constants.TargetedElements.Fronius.Data.kwId
                            };
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function started successfully.");
                            await dataWebScraper.GenericFetchPowerDataAsync(dataUrl, dashboardId, null, selectors, null, false, false);
                            _logger.LogInformation($"{dashboardId} Fetch Power Data function ended.");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (NoSuchWindowException ex)
            {
                _logger.LogError(ex, "Chrome instance was closed. Attempting to re-open.");
                try
                {
                    _chromedDriverManager.ReopenChromeDriver(dashboardId);
                    await FetchDataAsync(dataUrl, dashboardId);
                }
                catch (Exception ex1)
                {
                    _logger.LogError("Could not reopen the ChromeDriver windows." + ex1);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data.");
            }
        }
    }
}
