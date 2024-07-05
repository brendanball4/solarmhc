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
                    var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);
                    double utilizationPercentage = ((double)currentWattage / 25) * 100;

                    if (result)
                    {
                        // Return the utilizationPercentage & currentWattage values
                        await _liveDataService.UpdateCurrentPowerAsync(Constants.Names.Fronius, currentWattage);
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
                finally
                {
                    driver.Quit();
                }
            }
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

                    await _liveDataService.UpdateCurrentPowerAsync(Constants.Names.Huawei, currentPower);
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

        public async Task SolarEdgeStartFetchingPowerDataAsync(string dataUrl)
        {
            throw new NotImplementedException();
        }

        public async Task SunnyStartFetchingPowerDataAsync(string dataUrl)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var driver = scope.ServiceProvider.GetService<ChromeDriver>();
                try
                {
                    #region login
                    // Navigate to the data URL
                    driver.Navigate().GoToUrl(dataUrl);

                    var cookiePopup = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.cookiePopup));
                    cookiePopup.Click();

                    // Wait for the element with the id 'loginForm' to load
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(Constants.TargetedElements.Sunny.Auth.loginForm)));

                    // Find the username, password, and login fields
                    var usernameField = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.username));
                    var passwordField = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.password));
                    var loginButton = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.loginButton));

                    string username = Environment.GetEnvironmentVariable("MY_APP_SUNNY_USERNAME"); // setx <name> <data>
                    string password = Environment.GetEnvironmentVariable("MY_APP_SUNNY_PASSWORD");

                    // Enter the username and password
                    usernameField.SendKeys(username);
                    passwordField.SendKeys(password);

                    // Click the login button
                    loginButton.Click();
                    #endregion

                    #region Data scraping
                    // Wait for the element with the id 'loginForm' to load
                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(Constants.TargetedElements.Sunny.Data.kwId)));

                    // Find the element with the data
                    var powerElement = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Data.kwId));  // TODO: Change to constant
                    var currentPower = powerElement.Text;

                    var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);
                    double utilizationPercentage = ((double)currentWattage / 25) * 100;

                    if (result)
                    {
                        // Return the utilizationPercentage & currentWattage values
                        await _liveDataService.UpdateCurrentPowerAsync(Constants.Names.Sunny, currentWattage);
                    }
                    else
                    {
                        _logger.LogError("An error occurred while parsing the data.");
                    }
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
    }
}
