using OpenQA.Selenium.Chrome;

namespace solarmhc.Models
{
    public static class ChromeDriverFactory
    {
        public static ChromeDriver CreateChromeDriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var options = new ChromeOptions();

            //options.AddArgument("--headless");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--ignore-certificate-errors");

            return new ChromeDriver(chromeDriverService, options);
        }
    }
}
