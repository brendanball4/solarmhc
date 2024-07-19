using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper;

namespace solarmhc.Models.Background_Services
{
    public class WeatherBackgroundService : BackgroundService
    {
        private readonly ILogger<WeatherBackgroundService> _logger;
        private readonly WeatherService _weatherService;
        public WeatherBackgroundService(WeatherService weatherService, ILogger<WeatherBackgroundService> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting weather service.");
            stoppingToken.Register(() => _logger.LogInformation("Stopping web scraper services."));

            // Live data for viewing on the website, currently runs every 30 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                await _weatherService.FetchWeatherData("Medicine Hat");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Adjust the delay as needed
            }

            _logger.LogInformation("Weather service has stopped.");
        }
    }
}
