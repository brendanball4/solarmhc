using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models
{
    public class ChromeDriverFactory
    {
        public static ChromeDriver CreateChromeDriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();

            // Add any desired ChromeOptions here
            //options.AddArgument("--headless"); // Example: Run in headless mode
            options.AddArgument("--no-sandbox"); // Example: Disable sandboxing (useful for certain environments)
            options.AddArgument("--disable-gpu"); // Example: Disable GPU usage
            options.AddArgument("--disable-dev-shm-usage"); // Example: Overcome limited resource problems
            options.AddArgument("--ignore-certificate-errors");

            // Add the extension to the Chrome driver
            //options.AddArguments("load-extension=C:\\Users\\Brendan.Ball\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Extensions\\ofpnikijgfhlmmjlpkfaifhhdonchhoi\\1.0.3_1");

            return new ChromeDriver(chromeDriverService, options);
        }
    }
}
