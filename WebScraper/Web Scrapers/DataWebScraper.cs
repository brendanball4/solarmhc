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
                    var result = _webScraperHelper.TryParseData(currentPower, out decimal currentWattage);
                    double utilizationPercentage = ((double)currentWattage / 25) * 100;

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
            using (var scope = _serviceProvider.CreateScope())
            {
                var driver = scope.ServiceProvider.GetService<ChromeDriver>();
                try
                {
                    #region login
                    // Navigate to the data URL
                    driver.Navigate().GoToUrl(dataUrl);

                    // Wait for the element with the id 'loginForm' to load
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Constants.TargetedElements.Huawei.Auth.loginForm)));

                    // Find the username, password, and login fields
                    var usernameField = driver.FindElement(By.Id(Constants.TargetedElements.Huawei.Auth.username));
                    var passwordField = driver.FindElement(By.Id(Constants.TargetedElements.Huawei.Auth.password));
                    var loginButton = driver.FindElement(By.CssSelector(Constants.TargetedElements.Huawei.Auth.loginButton));

                    string username = Environment.GetEnvironmentVariable("MY_APP_HUA_USERNAME"); // setx <name> <data>
                    string password = Environment.GetEnvironmentVariable("MY_APP_HUA_PASSWORD");

                    // Enter the username and password
                    usernameField.SendKeys(username);
                    passwordField.SendKeys(password);

                    // Click the login button
                    loginButton.Click();
                    #endregion

                    #region Data scraping
                    // Wait for the iframe to load
                    wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.CssSelector("iframe#main_iframe_center"))); // TODO: Change to constant

                    // Find the element with the data
                    var powerElement = driver.FindElement(By.CssSelector("span#pvSystemOverviewPower"));  // TODO: Change to constant
                    var currentPower = decimal.Parse(powerElement.Text);

                    // Parse the data and enter the information into the database
                    double utilizationPercentage = ((double)currentPower / 25) * 100;

                    SubmitPowerIntakeData(Constants.Names.Huawei, utilizationPercentage, currentPower);
                    driver.Close();
                    #endregion
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions as necessary
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    driver.Close();
                    scope.Dispose();
                }
            }
        }

        public async Task SunnyStartFetchingPowerDataAsync(string dataUrl)
        {
        }
        #endregion

        private void SubmitPowerIntakeData(string dashboardName, double utilizationPercentage, decimal currentWattage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();

                // Get the solar segment with given name
                var solarSegment = dbContext.SolarSegments.FirstOrDefault(s => s.Name == dashboardName);

                // If the solar segment is not found, stop the service.
                if (solarSegment == null)
                {
                    _logger.LogError("Solar segment with the name '" + dashboardName + "' was not found. Stopping service.");
                    return;
                }

                // Create a new PowerIntake object
                PowerIntake powerIntake = new PowerIntake
                {
                    SolarSegment = solarSegment,
                    KW = Math.Round(currentWattage, 2),
                    Utilization = Math.Round(utilizationPercentage, 2),
                    TimeStamp = DateTime.Now
                };

                // Save the powerIntake to the database.
                dbContext.PowerIntakes.Add(powerIntake);
                dbContext.SaveChanges();
            }
        }
    }
}
