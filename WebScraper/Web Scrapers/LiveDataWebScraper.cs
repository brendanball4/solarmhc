using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace solarmhc.Models.Services.Web_Scrapers
{
    public class LiveDataWebScraper
    {
        private readonly ILogger _logger;
        private readonly ChromeDriverService _chromeDriverService;

        public LiveDataWebScraper(ILogger logger, ChromeDriverService chromeDriverService)
        {
            _logger = logger;
            _chromeDriverService = chromeDriverService;
        }

        public async Task<string> FetchFroniusPowerDataAsync(string dataUrl, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _chromeDriverService.Driver.Navigate().GoToUrl(dataUrl);

                    WebDriverWait wait = new WebDriverWait(_chromeDriverService.Driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(Constants.TargetedElements.Fronius)));

                    var powerElement = _chromeDriverService.Driver.FindElement(By.CssSelector(Constants.TargetedElements.Fronius));
                    return powerElement.Text;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching data from " + Constants.Names.Fronius);
                    _chromeDriverService?.Dispose();
                    return null;
                }
            }
        }

        internal void APSStartFetchingPowerDataAsync(string dataUrl, CancellationToken cts)
        {
            throw new NotImplementedException();
        }

        internal void FroniusStartFetchingPowerDataAsync(string dataUrl, CancellationToken cts)
        {
            throw new NotImplementedException();
        }

        internal void HuaweiStartFetchingPowerDataAsync(string dataUrl, CancellationToken cts)
        {
            throw new NotImplementedException();
        }

        internal void SolarEdgeStartFetchingPowerDataAsync(string dataUrl, CancellationToken cts)
        {
            throw new NotImplementedException();
        }

        internal void SunnyStartFetchingPowerDataAsync(string dataUrl, CancellationToken cts)
        {
            throw new NotImplementedException();
        }
    }
}
