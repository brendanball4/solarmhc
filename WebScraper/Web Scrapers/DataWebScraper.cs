using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Data;
using WebScraper.Models;

namespace WebScraper
{
    public class DataWebScraper
    {
        private IWebDriver driver;
        private readonly ILogger<DataWebScraper> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool isRunning;

        public DataWebScraper(IServiceProvider serviceProvider)
        {
            // Inject the service provider
            _serviceProvider = serviceProvider;

            // Set up the Chrome driver
            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--headless"); // Runs in headless mode. Commenting out for easier debugging.

            // Add the extension to the Chrome driver
            options.AddArguments("load-extension=C:\\Users\\Brendan.Ball\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\ofpnikijgfhlmmjlpkfaifhhdonchhoi\\1.0.3_1");

            // Create the Chrome driver with extensions and arguments
            driver = new ChromeDriver(options);
        }

        public async Task FroniusStartFetchingPowerData(string dataUrl)
        {
            try
            {
                // Navigate to the data URL
                driver.Navigate().GoToUrl(dataUrl);

                // Wait for the element with the class "js-status-bar-text" to be visible
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.js-status-bar-text")));

                // Find the element with the class "js-status-bar-text"
                var powerElement = driver.FindElement(By.CssSelector("div.js-status-bar-text"));
                var currentPower = powerElement.Text;

                // Parse the data and enter the information into the database
                var result = TryParseData(currentPower, out double utilizationPercentage, out decimal currentWattage);

                if (result)
                {
                    SubmitPowerIntakeData(solarmhc.Models.Constants.Names.Fronius, utilizationPercentage, currentWattage);
                    driver.Close();
                } else
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


        public async Task APSStartFetchingPowerData(string dataUrl)
        {
        }

        public async Task SolarEdgeStartFetchingPowerData(string dataUrl)
        {
        }

        public async Task HuaweiStartFetchingPowerData(string dataUrl)
        {
        }

        public async Task SunnyStartFetchingPowerData(string dataUrl)
        {
        }

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
                    ArrayName = solarSegment,
                    KW = currentWattage,
                    Utilization = utilizationPercentage,
                    TimeStamp = DateTime.Now
                };

                // Save the powerIntake to the database.
                dbContext.PowerIntakes.Add(powerIntake);
                dbContext.SaveChanges();
            }
        }

        private bool TryParseData(string data, out double utilizedPower, out decimal currentWattage)
        {
            utilizedPower = 0;
            currentWattage = 0;
            try
            {
                var lines = data.Split('\n');
                if (lines.Length >= 2)
                {
                    // Clean and parse the KW value
                    var kwString = lines[0].Replace("kW", "").Replace("\r", "").Trim();
                    // Clean and parse the Utilization value
                    var utilizationString = lines[1].Replace("Utilization", "").Replace("%", "").Trim();

                    if (decimal.TryParse(kwString, out currentWattage) &&
                        double.TryParse(utilizationString, out utilizedPower))
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


        public void StopFetching()
        {
            isRunning = false;
            driver.Quit();
        }
    }
}
