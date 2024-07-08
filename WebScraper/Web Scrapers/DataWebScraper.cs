using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading.Tasks;
using WebScraper;
using WebScraper.Data;
using WebScraper.Models;

namespace solarmhc.Models.Services.Web_Scrapers
{
    public class DataWebScraper
    {
        private readonly ILogger _logger;
        private readonly WebScraperHelperService _webScraperHelperService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LiveDataService _liveDataService;

        public DataWebScraper(WebScraperHelperService webScraperHelperService, IServiceProvider serviceProvider, LiveDataService liveDataService)
        {
            _webScraperHelperService = webScraperHelperService;
            _serviceProvider = serviceProvider;
            _liveDataService = liveDataService;
        }

        public async Task GenericFetchPowerDataAsync(string dataUrl, string dashboardId, ScrapingSelectors selectedElements, EScraper eScraper, AuthSelectors? authSelectors, bool cookies, bool iframe)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var driver = scope.ServiceProvider.GetRequiredService<ChromeDriver>();
                try
                {
                    // Navigate to the given URL
                    await Task.Run(() => driver.Navigate().GoToUrl(dataUrl));

                    if (cookies)
                    {
                        await Task.Run(() =>
                        {
                            var cookiePopup = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.cookiePopup));
                            cookiePopup.Click();
                        });
                    }

                    await Task.Run(() =>
                    {
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                        if (authSelectors != null)
                        {
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitConditionAuth)));

                            var authResult = TryAuth(authSelectors, driver);
                            if (!authResult)
                            {
                                _logger.LogError($"There was an error authenticating on {dashboardId}");
                                return;
                            }
                        }

                        if (iframe)
                        {
                            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.CssSelector("iframe#main_iframe_center")));
                        }

                        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitCondition)));
                    });

                    var powerElement = driver.FindElement(By.CssSelector(selectedElements.PowerField));
                    var currentPower = powerElement.Text;

                    var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);

                    if (eScraper == EScraper.Live)
                    {
                        if (result)
                        {
                            await _liveDataService.UpdateCurrentPowerAsync(dashboardId, currentWattage);
                        }
                        else
                        {
                            _logger.LogError($"Error capturing and setting live data for {dashboardId}");
                        }
                    }
                    else if (eScraper == EScraper.Data)
                    {
                        double utilizationPercentage = ((double)currentWattage / 25) * 100;

                        if (result)
                        {
                            SubmitPowerIntakeData(dashboardId, utilizationPercentage, currentWattage);
                            driver.Close();
                        }
                        else
                        {
                            _logger.LogError("An error occurred while parsing the data.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while fetching data for {dashboardId}.");
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

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

        private bool TryAuth(AuthSelectors authSelectors, ChromeDriver driver)
        {
            var usernameField = driver.FindElement(By.Id(authSelectors.UsernameField));
            var passwordField = driver.FindElement(By.Id(authSelectors.PasswordField));
            var loginButton = driver.FindElement(By.CssSelector(authSelectors.LoginButtonField));

            string username = Environment.GetEnvironmentVariable(authSelectors.EnvUsername);
            string password = Environment.GetEnvironmentVariable(authSelectors.EnvPassword);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Could not find username and password environment variables on this system.");
                return false;
            }

            usernameField.SendKeys(username);
            passwordField.SendKeys(password);
            loginButton.Click();

            return true;
        }
    }
}
