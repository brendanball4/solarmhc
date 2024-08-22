using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Services
{
    public class ChromeDriverManager
    {
        private Dictionary<string, ChromeDriver> _chromeDrivers = new Dictionary<string, ChromeDriver>();
        private readonly ILogger<ChromeDriverManager> _logger;

        public ChromeDriverManager(ILogger<ChromeDriverManager> logger)
        {
            InitializeChromeDrivers();
            _logger = logger;
        }

        private void InitializeChromeDrivers()
        {
            _chromeDrivers[$"{Constants.Names.Sunny}"] = CreateChromeDriver();
            _chromeDrivers[$"{Constants.Names.APS}"] = CreateChromeDriver();
            _chromeDrivers[$"{Constants.Names.Huawei}"] = CreateChromeDriver();
            _chromeDrivers[$"{Constants.Names.Fronius}"] = CreateChromeDriver();
        }

        public void ReopenChromeDriver(string dashboardId)
        {
            switch (dashboardId)
            {
                case Constants.Names.Sunny:
                    _chromeDrivers[$"{Constants.Names.Sunny}"] = CreateChromeDriver();
                    break;
                case Constants.Names.APS:
                    _chromeDrivers[$"{Constants.Names.APS}"] = CreateChromeDriver();
                    break;
                case Constants.Names.Huawei:
                    _chromeDrivers[$"{Constants.Names.Huawei}"] = CreateChromeDriver();
                    break;
                case Constants.Names.Fronius:
                    _chromeDrivers[$"{Constants.Names.Fronius}"] = CreateChromeDriver();
                    break;
                default:
                    break;
            }
        }

        private ChromeDriver CreateChromeDriver()
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

        public ChromeDriver GetDriver(string dashboardId)
        {
            _logger.LogInformation($"{dashboardId}: Finding ChromeDriver.");
            if (_chromeDrivers.TryGetValue(dashboardId, out var driver))
            {
                _logger.LogInformation($"{dashboardId}: Found ChromeDriver.");
                return driver;
            }
            _logger.LogError($"{dashboardId}: Not able to find a ChromeDriver instance.");
            throw new KeyNotFoundException($"No ChromeDriver instance found for dashboard: {dashboardId}");
        }

        public void DisposeDrivers()
        {
            foreach (var driver in _chromeDrivers.Values)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }

}
