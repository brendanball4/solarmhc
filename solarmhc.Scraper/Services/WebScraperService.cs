using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V125.Debugger;
using OpenQA.Selenium.DevTools.V125.Media;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using solarmhc.Scraper.Data;
using solarmhc.Scraper.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace solarmhc.Scraper.Services
{
    public class WebScraperService
    {
        private readonly ILogger<WebScraperService> _logger;
        private readonly ChromeDriverManager _driverManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly WebScraperHelperService _webScraperHelperService;
        private readonly HttpClient _httpClient;

        public WebScraperService(IServiceProvider serviceProvider, HttpClient httpClient, ILogger<WebScraperService> logger, ChromeDriverManager chromeDriverManager, WebScraperHelperService webScraperHelperService)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _logger = logger;
            _driverManager = chromeDriverManager;
            _webScraperHelperService = webScraperHelperService;
        }

        public async Task GenericFetchPowerDataAsync(string dataUrl, string dashboardId, ScrapingSelectors selectedElements, AuthSelectors? authSelectors, bool cookies, bool iframe)
        {
            _logger.LogInformation($"{dashboardId}: Finding ChromeDriver.");
            var driver = _driverManager.GetDriver(dashboardId);
            _logger.LogInformation($"{dashboardId}: Found ChromeDriver.");

            bool loggedIn = false;

            switch (dashboardId)
            {
                case Constants.Names.APS:
                    if (driver.Url == "https://apsystemsema.com/ema/security/optsecondmenu/intoViewOptModule.action")
                    {
                        driver.Navigate().Refresh();
                        loggedIn = true;
                    }
                    break;
                case Constants.Names.Sunny:
                    if (driver.Url == "https://ennexos.sunnyportal.com/11382962/dashboard")
                    {
                        driver.Navigate().Refresh();
                        loggedIn = true;
                    }
                    break;
                case Constants.Names.Huawei:
                    if (driver.Url == "https://huawei.frankensolar.ca:8443/securitys!tologin.action")
                    {
                        loggedIn = true;
                    }
                    break;
                case Constants.Names.Fronius:
                    if (driver.Url.Contains("https://www.solarweb.com/PvSystems/PvSystem?pvSystemId="))
                    {
                        loggedIn = true;
                    }
                    break;
                default:
                    break;
            }

            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                if (!loggedIn)
                {
                    // Navigate to the given URL
                    await Task.Run(() => driver.Navigate().GoToUrl(dataUrl));

                    if (cookies)
                    {
                        _logger.LogInformation($"{dashboardId}: Checking for cookies popup.");
                        await Task.Run(() =>
                        {
                            try
                            {
                                var cookiePopup = driver.FindElement(By.CssSelector(Constants.TargetedElements.Sunny.Auth.cookiePopup));
                                cookiePopup.Click();
                                _logger.LogInformation($"{dashboardId}: Found and dismissed cookies popup.");
                            }
                            catch (Exception)
                            {
                                _logger.LogError($"{dashboardId}: Error finding and dismissing the cookies popup.");
                            }

                        });
                    }

                    var taskResult = await Task.Run(() =>
                    {
                        // Check if the website requires a login process
                        if (authSelectors != null)
                        {
                            _logger.LogInformation($"{dashboardId}: Waiting for element to be visible.");
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitConditionAuth)));
                            _logger.LogInformation($"{dashboardId}: Element found.");

                            _logger.LogInformation($"{dashboardId}: Attempting Authentication.");
                            bool authResult = TryAuth(dashboardId, authSelectors, driver);

                            if (!authResult)
                            {
                                _logger.LogError($"There was an error authenticating on {dashboardId}");
                                return false;
                            }

                            _logger.LogInformation($"{dashboardId}: Authentication successful.");
                        }

                        if (iframe)
                        {
                            _logger.LogInformation($"{dashboardId}: Looking for iframe.");
                            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.CssSelector("iframe#main_iframe_center")));
                            _logger.LogInformation($"{dashboardId}: Found and swapped over to iframe.");
                        }

                        return true;
                    });
                    if (!taskResult)
                    {
                        return; // Exit the method if any task inside the Task.Run block returns false
                    }
                }
                if (dashboardId == Constants.Names.APS)
                {
                    _logger.LogInformation($"{dashboardId}: Starting Fetch for APS power data.");
                    await FetchPowerDataAPS(driver, dashboardId, wait, loggedIn);
                    _logger.LogInformation($"{dashboardId}: Ending fetch for APS power data.");
                    return;
                }

                _logger.LogInformation($"{dashboardId}: Waiting for element to load.");
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitCondition)));
                _logger.LogInformation($"{dashboardId}: Found element.");

                _logger.LogInformation($"{dashboardId}: Selecting power field.");
                var powerElement = driver.FindElement(By.CssSelector(selectedElements.PowerField));
                var currentPower = powerElement.Text;

                var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);
                _logger.LogInformation($"{dashboardId}: Current kW = {currentWattage}.");

                if (currentWattage > 30)
                {
                    _logger.LogInformation($"{dashboardId}: System is currently displaying watts, converting to kW.");
                    // Change from Watts to kW for systems that report watts at low utilization
                    currentWattage /= 1000;
                }

                // Create utilization percentage based on the current wattage
                var utilPercentage = (currentWattage / 25) * 100;
                _logger.LogInformation($"{dashboardId}: Utilization % = {utilPercentage}, Current wattage = {currentWattage}.");
                // Submit the data to the database
                SubmitPowerIntakeData(dashboardId, (double)utilPercentage, currentWattage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching data for {dashboardId}.");
            }
        }

        private async Task FetchPowerDataAPS(ChromeDriver driver, string dashboardId, WebDriverWait wait, bool loggedIn)
        {
            try
            {
                if (!loggedIn)
                {
                    driver.Navigate().GoToUrl("https://apsystemsema.com/ema/security/optsecondmenu/intoViewOptModule.action");
                }

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

                SubmitPowerIntakeData(dashboardId, currentUtilization, (decimal)currentKw);
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
                    var currentUtilization = (currentPowerKw / Constants.Capacities.SolarEdge) * 100;

                    // Collect data for live viewing, and saving to database
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

        private void SubmitPowerIntakeData(string dashboardId, double utilizationPercentage, decimal currentWattage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _logger.LogInformation($"{dashboardId}: Finding database service.");
                var dbContext = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();
                _logger.LogInformation($"{dashboardId}: Found dataabase service.");

                _logger.LogInformation($"{dashboardId}: Looking for the associated solar segment.");
                // Get the solar segment with given name
                var solarSegment = dbContext.SolarSegments.FirstOrDefault(s => s.Name == dashboardId);
                _logger.LogInformation($"{dashboardId}: Found the associated solar segment.");

                // If the solar segment is not found, stop the service.
                if (solarSegment == null)
                {
                    _logger.LogError("Solar segment with the name '" + dashboardId + "' was not found. Stopping service.");
                    return;
                }

                _logger.LogInformation($"{dashboardId}: Creating PowerIntake object with given data.");
                // Create a new PowerIntake object
                PowerIntake powerIntake = new PowerIntake
                {
                    SolarSegment = solarSegment,
                    KW = Math.Round(currentWattage, 2),
                    Utilization = Math.Round(utilizationPercentage, 2),
                    TimeStamp = DateTime.Now
                };
                _logger.LogInformation($"{dashboardId}: Created PowerIntake Object.");

                _logger.LogInformation($"{dashboardId}: Adding values to the database.");
                // Save the powerIntake to the database.
                dbContext.PowerIntakes.Add(powerIntake);
                dbContext.SaveChanges();
                _logger.LogInformation($"{dashboardId}: Values submitted to database.");
            }
        }

        private bool TryAuth(string dashboardId, AuthSelectors authSelectors, ChromeDriver driver)
        {
            _logger.LogInformation($"{dashboardId}: Finding authentication elements.");
            var usernameField = driver.FindElement(By.Id(authSelectors.UsernameField));
            var passwordField = driver.FindElement(By.Id(authSelectors.PasswordField));
            var loginButton = driver.FindElement(By.CssSelector(authSelectors.LoginButtonField));
            _logger.LogInformation($"{dashboardId}: Found authentication elements.");

            _logger.LogInformation($"{dashboardId}: Grabbing username & password from environment variables.");
            string username = Environment.GetEnvironmentVariable(authSelectors.EnvUsername);
            string password = Environment.GetEnvironmentVariable(authSelectors.EnvPassword);
            _logger.LogInformation($"{dashboardId}: Found username & password from environment variables.");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Could not find username and password environment variables on this system.");
                return false;
            }

            _logger.LogInformation($"{dashboardId}: Bot is entering login information.");
            usernameField.SendKeys(username);
            passwordField.SendKeys(password);
            loginButton.Click();
            _logger.LogInformation($"{dashboardId}: Bot is attempting login.");

            return true;
        }
    }
}
