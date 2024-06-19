using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class ChromeDriverService : IDisposable
    {
        public IWebDriver Driver { get; private set; }

        public ChromeDriverService()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--headless"); // Runs in headless mode. Comment out for easier debugging.

            // Add the extension to the Chrome driver
            options.AddArguments("load-extension=C:\\Users\\Brendan.Ball\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\ofpnikijgfhlmmjlpkfaifhhdonchhoi\\1.0.3_1");

            Driver = new ChromeDriver(options);
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
        }
    }
}
