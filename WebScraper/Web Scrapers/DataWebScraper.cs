using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using solarmhc.Models.Services;
using WebScraper.Data;
using WebScraper.Models;

namespace solarmhc.Models.Services
{
    public class DataWebScraper
    {
        private readonly ILogger<DataWebScraper> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WebScraperHelperService _webScraperHelper;
        private bool isRunning;

        public DataWebScraper(IServiceProvider serviceProvider, WebScraperHelperService webScraperHelper)
        {
            // Inject the service provider
            _serviceProvider = serviceProvider;
            _webScraperHelper = webScraperHelper;
        }

        #region DB Data Scrapers
        public async Task FroniusStartFetchingPowerDataAsync(string dataUrl)
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
                    var result = _webScraperHelper.TryParseData(currentPower, out double utilizationPercentage, out decimal currentWattage);

                    if (result)
                    {
                        SubmitPowerIntakeData(Constants.Names.Fronius, utilizationPercentage, currentWattage);
                        driver.Close();
                    }
                    else
                    {
                        _logger.LogError("An error occurred while parsing the data.");
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions as necessary
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task APSStartFetchingPowerDataAsync(string dataUrl)
        {
        }

        public async Task SolarEdgeStartFetchingPowerDataAsync(string dataUrl)
        {
        }

        public async Task HuaweiStartFetchingPowerDataAsync(string dataUrl)
        {
        }

        public async Task SunnyStartFetchingPowerDataAsync(string dataUrl)
        {
        }
        #endregion

        private void SubmitPowerIntakeData(string inverterName, double utilizationPercentage, decimal currentWattage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();

                // Get the solar segment with given name
                var solarSegment = dbContext.SolarSegments.FirstOrDefault(s => s.Name == inverterName);

                // If the solar segment is not found, stop the service.
                if (solarSegment == null)
                {
                    _logger.LogError("Solar segment with the name '" + inverterName + "' was not found. Stopping service.");
                    return;
                }

                // Create a new PowerIntake object
                PowerIntake powerIntake = new PowerIntake
                {
                    SolarSegment = solarSegment,
                    KW = currentWattage,
                    Utilization = utilizationPercentage,
                    TimeStamp = DateTime.Now
                };

                // Save the powerIntake to the database.
                dbContext.PowerIntakes.Add(powerIntake);
                dbContext.SaveChanges();
            }
        }
    }
}
