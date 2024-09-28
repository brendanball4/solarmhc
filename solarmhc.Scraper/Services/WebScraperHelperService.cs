using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Services
{
    public class WebScraperHelperService
    {
        private readonly ILogger<WebScraperHelperService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WebScraperHelperService(ILogger<WebScraperHelperService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public bool TryParseData(string data, out decimal currentWattage)
        {
            var kwString = "";
            currentWattage = 0;
            try
            {
                var lines = data.Split('\n'); 
                if (lines.Length >= 1)
                {
                    // Clean and parse the W value
                    if (data.Contains("kW"))
                    {
                        // Clean and parse the KW value
                        kwString = lines[0].Replace("kW", "").Replace("\r", "").Trim();
                    } else if (data.Contains(" W"))
                    {
                        kwString = lines[0].Replace("W", "").Replace("\r", "").Trim();
                    } 
                    else
                    {
                        kwString = lines[0];
                    }

                    if (decimal.TryParse(kwString, out currentWattage))
                    {
                        if (data.Contains(" W"))
                        {
                            currentWattage /= 1000;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while parsing the data.");
            }
            return false;
        }
    }
}
