using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace solarmhc.Models.Services.Web_Scrapers
{
    public class LiveDataWebScraper
    {
        private readonly ILogger _logger;
        private readonly WebScraperHelperService _webScraperHelperService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LiveDataService _liveDataService;

        public LiveDataWebScraper(ILogger logger, WebScraperHelperService webScraperHelperService, IServiceProvider serviceProvider, LiveDataService liveDataService)
        {
            _logger = logger;
            _webScraperHelperService = webScraperHelperService;
            _serviceProvider = serviceProvider;
            _liveDataService = liveDataService;
        }

        public async Task<string> APSStartFetchingPowerDataAsync(string dataUrl)
        {
            throw new NotImplementedException();
        }

        public async Task<(double utilizationPercentage, decimal currentWattage)> FroniusStartFetchingPowerDataAsync(string dataUrl)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var driver = scope.ServiceProvider.GetService<ChromeDriver>();
                try
                {
                    // Navigate to the data URL
                    driver.Navigate().GoToUrl(dataUrl);

                    // Wait for the element with the class "js-status-bar-text" to be visible
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(Constants.TargetedElements.Fronius)));

                    // Find the element with the class "js-status-bar-text"
                    var powerElement = driver.FindElement(By.CssSelector(Constants.TargetedElements.Fronius));
                    var currentPower = powerElement.Text;

                    // Parse the data and enter the information into the database
                    var result = _webScraperHelperService.TryParseData(currentPower, out double utilizationPercentage, out decimal currentWattage);

                    if (result)
                    {
                        // Return the utilizationPercentage & currentWattage values
                        await _liveDataService.UpdateCurrentPowerAsync(utilizationPercentage, currentWattage);
                        return (utilizationPercentage, currentWattage);
                    }
                    else
                    {
                        _logger.LogError("An error occurred while parsing the data.");
                        return (0, 0);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions as necessary
                    Console.WriteLine($"Error: {ex.Message}");
                    return (0, 0);
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        public async Task<string> HuaweiStartFetchingPowerDataAsync(string dataUrl)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SolarEdgeStartFetchingPowerDataAsync(string dataUrl)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SunnyStartFetchingPowerDataAsync(string dataUrl)
        {
            throw new NotImplementedException();
        }
    }
}
