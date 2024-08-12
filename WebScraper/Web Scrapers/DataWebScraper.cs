using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebScraper;
using WebScraper.Data;
using WebScraper.Models;

namespace solarmhc.Models.Services.Web_Scrapers
{
    public class DataWebScraper
    {
        private readonly ILogger<DataWebScraper> _logger;
        private readonly WebScraperHelperService _webScraperHelperService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LiveDataService _liveDataService;
        private readonly HttpClient _httpClient;

        public DataWebScraper(WebScraperHelperService webScraperHelperService, IServiceProvider serviceProvider, LiveDataService liveDataService, HttpClient httpClient, ILogger<DataWebScraper> logger)
        {
            _webScraperHelperService = webScraperHelperService;
            _serviceProvider = serviceProvider;
            _liveDataService = liveDataService;
            _httpClient = httpClient;
            _logger = logger;
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

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    var taskResult = await Task.Run(() =>
                    {

                        if (authSelectors != null)
                        {
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitConditionAuth)));

                            var authResult = TryAuth(authSelectors, driver);
                            if (!authResult)
                            {
                                _logger.LogError($"There was an error authenticating on {dashboardId}");
                                return false;
                            }
                        }

                        if (iframe)
                        {
                            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.CssSelector("iframe#main_iframe_center")));
                        }

                        return true;
                    });
                    if (dashboardId == Constants.Names.APS)
                    {
                        FetchPowerDataAPS(driver, eScraper, dashboardId, wait);
                    }

                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitCondition)));

                    if (!taskResult)
                    {
                        return; // Exit the method if any task inside the Task.Run block returns false
                    }

                    var powerElement = driver.FindElement(By.CssSelector(selectedElements.PowerField));
                    var currentPower = powerElement.Text;

                    var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);

                    if (currentWattage > 25)
                    {
                        // Change from Watts to kW for systems that report watts at low utilization
                        currentWattage /= 1000;
                    }

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
                            await _liveDataService.UpdatePowerDataAsync(dashboardId);
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
                    driver.Close();
                    driver.Quit();
                }
            }
        }

        private async Task FetchPowerDataAPS(ChromeDriver driver, EScraper eScraper, string dashboardId, WebDriverWait wait)
        {
            try
            {
                driver.Navigate().GoToUrl("https://apsystemsema.com/ema/security/optsecondmenu/intoViewOptModule.action");

                // Ensure the driver session is still active
                if (driver.SessionId == null)
                {
                    throw new WebDriverException("WebDriver session is invalid.");
                }

                await Task.Run(() => wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div#module0"))));

                List<double> wattageValues = new List<double>();

                // Adjust the range based on the number of panels
                for (int i = 0; i <= 62; i++)
                {
                    try
                    {
                        // Ensure the driver session is still active before each find
                        if (driver.SessionId == null)
                        {
                            throw new WebDriverException("WebDriver session is invalid.");
                        }

                        // Locate the wattage element
                        IWebElement wattageElement = driver.FindElement(By.Id($"module{i}"));

                        // Parse the wattage value and add to the list
                        if (double.TryParse(wattageElement.Text, out double wattage))
                        {
                            wattageValues.Add(wattage);
                        }
                        else
                        {
                            Console.WriteLine($"Could not parse wattage for panel {i}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error fetching wattage for panel {i}: {e.Message}");
                    }
                }

                double currentKw = wattageValues.Sum() / 1000; // Convert to kW
                var currentUtilization = (currentKw / Constants.Capacities.APS) * 100;

                if (eScraper == EScraper.Live)
                {
                    _liveDataService.SetCurrentPower(dashboardId, (decimal)currentKw);
                }
                else
                {
                    SubmitPowerIntakeData(dashboardId, currentUtilization, (decimal)currentKw);
                }
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine($"WebDriver exception: {ex.Message}");
                // Handle the WebDriver exception (e.g., reinitialize the WebDriver session)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General exception: {ex.Message}");
                // Handle other exceptions
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }


        public async Task FetchPowerDataSolarEdgeAPI()
        {
            string apiKey = Environment.GetEnvironmentVariable(Constants.EnvironmentVars.EnvironmentNames.SolarEdgeApi);
            string siteValue = Environment.GetEnvironmentVariable(Constants.EnvironmentVars.EnvironmentNames.SolarEdgeSite);

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(siteValue))
            {
                _logger.LogError("There was an error with collecting the API key for Solar Edge. (Make sure environment variables are set properly)");
            }

            string solarEdgeApiUrl = $"https://monitoringapi.solaredge.com/site/{siteValue}/overview?api_key={apiKey}";

            try
            {
                // Send the GET request
                HttpResponseMessage response = await _httpClient.GetAsync(solarEdgeApiUrl);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Response content: " + content);

                    var jsonResponse = JObject.Parse(content);

                    // Parse the JSON response
                    var currentPower = jsonResponse["overview"]["currentPower"]["power"].Value<double>();
                    var currentPowerKw = currentPower / 1000;
                    var currentUtilization = currentPowerKw / Constants.Capacities.SolarEdge;

                    // Collect data for live viewing, and saving to database
                    await _liveDataService.UpdateCurrentPowerAsync(Constants.Names.SolarEdge, (decimal)currentPowerKw);
                    SubmitPowerIntakeData(Constants.Names.SolarEdge, currentUtilization, (decimal)currentPowerKw);
                }
                else
                {
                    _logger.LogError($"Error: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error content: " + errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching data from SolarEdge: {ex.Message}");
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
