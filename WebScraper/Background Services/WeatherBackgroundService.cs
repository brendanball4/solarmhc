using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using solarmhc.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models.Background_Services
{
    public class WeatherBackgroundService : BackgroundService
    {
        private readonly ILogger<WeatherBackgroundService> _logger;
        private readonly WeatherApiService _weatherService;
        private readonly LiveDataService _liveDataService;
        public WeatherBackgroundService(WeatherApiService weatherService, ILogger<WeatherBackgroundService> logger, LiveDataService liveDataService)
        {
            _weatherService = weatherService;
            _logger = logger;
            _liveDataService = liveDataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting weather service.");
            stoppingToken.Register(() => _logger.LogInformation("Stopping weather service."));

            // Live data for viewing on the website, currently runs every 30 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                await _liveDataService.SetCurrentWeather("Medicine Hat");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Adjust the delay as needed
            }

            _logger.LogInformation("Weather service has stopped.");
        }
    }
}
