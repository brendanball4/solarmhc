using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V125.Debugger;
using OpenQA.Selenium.DevTools.V125.LayerTree;
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
        private const int maxAttempts = 3;

        public WebScraperService(IServiceProvider serviceProvider, HttpClient httpClient, ILogger<WebScraperService> logger, ChromeDriverManager chromeDriverManager, WebScraperHelperService webScraperHelperService)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _logger = logger;
            _driverManager = chromeDriverManager;
            _webScraperHelperService = webScraperHelperService;
        }

        public async Task GenericFetchPowerDataAsync(string dataUrl, string dashboardId, string? dataPage, ScrapingSelectors selectedElements, AuthSelectors? authSelectors, bool cookies, bool iframe)
        {
            // Attempt to grab the ChromeDriver instance for the given dashboardId
            var driver = _driverManager.GetDriver(dashboardId);

            // Create a wait object for the webdriver
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            // Checking to see the current status of the given webpage.
            CheckCurrentPage(dashboardId, driver, dataPage, out bool loggedIn);

            // Run login logic for the dashboards not logged in.
            if (!loggedIn)
            {
                try
                {
                    // Navigate to the given URL
                    await Task.Run(() => driver.Navigate().GoToUrl(dataUrl));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{dashboardId} | {CurrentDateTime()}: There was an error loading the page. Error = {ex}");
                    _logger.LogError($"{dashboardId} | {CurrentDateTime()}: Currently having issues loading the page. The failing system may be offline.");
                    // Mark system as offline
                    SubmitPowerIntakeData(dashboardId, 0, 0, false);
                    return;
                }

                // Confirm the driver is on the correct page
                CheckCurrentPage(dashboardId, driver, dataUrl, out bool correctPage);

                if (!correctPage)
                {
                    // Let system know that the driver is NOT on the login page.
                    _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Driver is currenty on the wrong page.");

                    for (int i = 0; i < maxAttempts; i++)
                    {
                        try
                        {
                            // Navigate to the given URL
                            await Task.Run(() => driver.Navigate().GoToUrl(dataUrl));

                            // Confirm the driver is on the correct page
                            CheckCurrentPage(dashboardId, driver, dataUrl, out bool correctPageLoop);

                            if (correctPageLoop)
                            {
                                _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Getting to the login page took {i} attempts.");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            _logger.LogError($"{dashboardId} | {CurrentDateTime()}: Failed to load {dataUrl}");
                        }
                    }

                    _logger.LogError($"{dashboardId} | {CurrentDateTime()}: Failed to load {dataUrl}");
                    return;
                }

                // Currently Sunny only logic
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
                            _logger.LogError($"{dashboardId}: Error finding the cookies popup.");
                        }
                    });
                }

                var taskResult = await Task.Run(() =>
                {
                    // Check if the website requires a login process
                    if (authSelectors != null)
                    {
                        bool authResult = TryAuth(dashboardId, authSelectors, driver, wait, selectedElements);

                        if (!authResult)
                        {
                            _logger.LogError($"There was an error authenticating on {dashboardId}");
                            return false;
                        }

                        _logger.LogInformation($"{dashboardId}: Authentication successful.");
                    }

                    // Currently Huawei only logic
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

            try
            {
                if (dashboardId == Constants.Names.APS)
                {
                    // Run APS specific logic and return out of this entire function.
                    await FetchPowerDataAPS(driver, dashboardId, wait, loggedIn);
                    return;
                }

                // Look for wait condition of main page to load
                _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Waiting for element to load.");
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitCondition)));
                _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Found element.");

                // Look for power element
                _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Selecting power field.");
                var powerElement = driver.FindElement(By.CssSelector(selectedElements.PowerField));
                var currentPower = powerElement.Text;

                // Try to get decimal value out of the power field string
                var result = _webScraperHelperService.TryParseData(currentPower, out decimal currentWattage);

                // If parsing fails to convert to decimal, then display that the system is currently offline
                if (!result)
                {
                    // Mark system as offline
                    SubmitPowerIntakeData(dashboardId, 0, 0, false);  // Pass 'false' for SystemStatus
                    _logger.LogError($"{dashboardId} | {CurrentDateTime()}: System offline. currentPower = {currentPower}.");
                }

                _logger.LogDebug($"{dashboardId} | {CurrentDateTime()}: Current kW = {currentWattage}.");

                if (currentWattage > 30)
                {
                    _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: System is currently displaying watts, converting to kW.");
                    // Change from Watts to kW for systems that report watts at low utilization
                    currentWattage /= 1000;
                }

                // Create utilization percentage based on the current wattage
                var utilPercentage = (currentWattage / 25) * 100;
                _logger.LogInformation($"{dashboardId} | {CurrentDateTime()}: Utilization % = {utilPercentage}, Current wattage = {currentWattage}.");
                // Submit the data to the database
                SubmitPowerIntakeData(dashboardId, (double)utilPercentage, currentWattage, true);
            }
            catch (Exception ex)
            {
                if (ex is WebDriverTimeoutException)
                {
                    switch (dashboardId)
                    {
                        case Constants.Names.Sunny:
                            selectedElements.WaitCondition = "span.ennexos-message";
                            selectedElements.PowerField = "span.ennexos-message";
                            await GenericFetchPowerDataAsync(dataUrl, dashboardId, Constants.DataUrls.DataPages.Sunny, selectedElements, authSelectors, true, false);
                            break;
                        case Constants.Names.APS:
                            break;
                        case Constants.Names.Huawei:
                            break;
                        case Constants.Names.Fronius:
                            break;
                        default:
                            break;
                    }
                }
                _logger.LogError(ex, $"An error occurred while fetching data for {dashboardId}.");
            }
        }

        private string CurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
        }

        private void CheckCurrentPage(string dashboardId, ChromeDriver driver, string page, out bool correctPage)
        {
            correctPage = false;

            switch (dashboardId)
            {
                case Constants.Names.APS:
                    // Login Page
                    if (driver.Url == page)
                    {
                        if (page == Constants.DataUrls.DataPages.APSData)
                        {
                            driver.Navigate().Refresh();
                        }
                        correctPage = true;
                    }
                    break;
                case Constants.Names.Sunny:
                    // Login Page
                    if (driver.Url == page)
                    {
                        driver.Navigate().Refresh();
                        correctPage = true;
                    }
                    break;
                case Constants.Names.Huawei:
                    // Login Page
                    if (driver.Url == page)
                    {
                        correctPage = true;
                    }
                    break;
                case Constants.Names.Fronius:
                    if (driver.Url.Contains("https://www.solarweb.com/PvSystems/PvSystem?pvSystemId="))
                    {
                        driver.Navigate().Refresh();
                        correctPage = true;
                    }
                    break;
                default:
                    break;
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
                    SubmitPowerIntakeData(Constants.Names.SolarEdge, currentUtilization, (decimal)currentPowerKw, true);
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

        private async Task FetchPowerDataAPS(ChromeDriver driver, string dashboardId, WebDriverWait wait, bool loggedIn)
        {
            try
            {
                _logger.LogInformation($"{dashboardId}: Starting Fetch for APS power data.");
                if (!loggedIn)
                {
                    // Wait for the main dashboard screen to confirm it is running before going to the new page.
                    await Task.Run(() => wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("a#today"))));
                    driver.Navigate().GoToUrl("https://apsystemsema.com/ema/security/optsecondmenu/intoViewOptModule.action");
                }

                try
                {
                    await Task.Run(() => wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div#module0"))));
                }
                catch (WebDriverTimeoutException ex)
                {
                    // SYSTEM OFFLINE OR ERROR FINDING MODULES
                    _logger.LogError("There was an error finding the elements on the screen. Error: " + ex);
                }
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

                SubmitPowerIntakeData(dashboardId, currentUtilization, (decimal)currentKw, true);
                _logger.LogInformation($"{dashboardId}: Ending fetch for APS power data.");
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

        private void SubmitPowerIntakeData(string dashboardId, double utilizationPercentage, decimal currentWattage, bool status)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // Create access to database
                _logger.LogInformation($"{dashboardId}: Finding database service.");
                var dbContext = scope.ServiceProvider.GetRequiredService<SolarMHCDbContext>();
                _logger.LogInformation($"{dashboardId}: Found dataabase service.");

                // Get the solar segment with given name
                _logger.LogInformation($"{dashboardId}: Looking for the associated solar segment.");
                var solarSegment = dbContext.SolarSegments.FirstOrDefault(s => s.Name == dashboardId);
                _logger.LogInformation($"{dashboardId}: Found the associated solar segment.");

                // If the solar segment is not found, stop the service.
                if (solarSegment == null)
                {
                    _logger.LogError($"Solar segment with the name {dashboardId} was not found. Stopping service.");
                    return;
                }

                // Create a new PowerIntake object
                _logger.LogInformation($"{dashboardId}: Creating PowerIntake object with given data.");
                PowerIntake powerIntake = new PowerIntake
                {
                    SolarSegment = solarSegment,
                    KW = Math.Round(currentWattage, 2),
                    Utilization = Math.Round(utilizationPercentage, 2),
                    TimeStamp = DateTime.Now,
                    Status = status
                };
                _logger.LogInformation($"{dashboardId}: Created PowerIntake Object.");

                // Save the powerIntake to the database.
                _logger.LogInformation($"{dashboardId}: Adding values to the database.");
                dbContext.PowerIntakes.Add(powerIntake);
                dbContext.SaveChanges();
                _logger.LogInformation($"{dashboardId}: Values submitted to database.");
            }
        }

        private bool TryAuth(string dashboardId, AuthSelectors authSelectors, ChromeDriver driver, WebDriverWait wait, ScrapingSelectors selectedElements)
        {
            try
            {
                _logger.LogInformation($"{dashboardId}: Waiting for element to be visible.");
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(selectedElements.WaitConditionAuth)));
                _logger.LogInformation($"{dashboardId}: Element found.");

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
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
