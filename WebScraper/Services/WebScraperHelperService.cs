using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class WebScraperHelperService
    {
        private readonly ILogger<WebScraperHelperService> _logger;

        public WebScraperHelperService(ILogger<WebScraperHelperService> logger)
        {
            _logger = logger;
        }

        public bool TryParseData(string data, out double utilizedPower, out decimal currentWattage)
        {
            utilizedPower = 0;
            currentWattage = 0;
            try
            {
                var lines = data.Split('\n');
                if (lines.Length >= 2)
                {
                    // Clean and parse the KW value
                    var kwString = lines[0].Replace("kW", "").Replace("\r", "").Trim();
                    // Clean and parse the W value
                    kwString = lines[0].Replace("W", "").Replace("\r", "").Trim();
                    // Clean and parse the Utilization value
                    var utilizationString = lines[1].Replace("Utilization", "").Replace("%", "").Trim();

                    if (decimal.TryParse(kwString, out currentWattage) &&
                        double.TryParse(utilizationString, out utilizedPower))
                    {
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
