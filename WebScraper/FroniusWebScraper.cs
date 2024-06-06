using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraper
{
    public class FroniusWebScraper
    {
        private IWebDriver driver;
        private bool isRunning;

        public FroniusWebScraper()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            //options.AddArgument("--headless"); // Runs in headless mode. Commenting out for easier debugging.

            options.AddArguments("load-extension=C:\\Users\\Brendan.Ball\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\ofpnikijgfhlmmjlpkfaifhhdonchhoi\\1.0.3_0");
            driver = new ChromeDriver(options);
        }

        public async Task StartFetchingPowerAsync(string dataUrl, CancellationToken token, Action<string> updateCallback)
        {
            try
            {
                // Navigate to the data URL
                driver.Navigate().GoToUrl(dataUrl);

                // Wait for the element with the class "js-status-bar-text" to be visible
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div.js-status-bar-text")));

                isRunning = true;
                while (isRunning && !token.IsCancellationRequested)
                {
                    // Find the element with the class "js-status-bar-text"
                    var powerElement = driver.FindElement(By.CssSelector("div.js-status-bar-text"));
                    var currentPower = powerElement.Text;

                    // Update the data via callback
                    updateCallback(currentPower);

                    // Wait for 1 second before the next iteration
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as necessary
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void StopFetching()
        {
            isRunning = false;
            driver.Quit();
        }
    }
}
