using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading.Tasks;

namespace WebScraper
{
    public class WebScraperService
    {
        public async Task<string> GetCurrentPowerAsync(string dataUrl)
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            //options.AddArgument("user-data-dir=C:\\Users\\Brendan.Ball\\AppData\\Local\\Google\\Chrome\\User Data"); // Temporarily disable
            // options.AddArgument("--profile-directory=Default"); // Temporarily disable

            try
            {
                using (var driver = new ChromeDriver(options))
                {
                    // Navigate to the data URL
                    driver.Navigate().GoToUrl(dataUrl);

                    // Find the element with the class "js-status-bar-text"
                    var powerElement = driver.FindElement(By.CssSelector("div.js-status-bar-text"));
                    var currentPower = powerElement.Text;

                    return currentPower;
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as necessary
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
