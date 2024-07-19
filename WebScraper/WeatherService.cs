using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using solarmhc.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace solarmhc.Models
{
    public class WeatherService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly LiveDataService _liveDataService;
        public WeatherService(ILogger<WeatherService> logger, LiveDataService liveDataService)
        {
            _logger = logger;
            _liveDataService = liveDataService;
        }

        public async Task FetchWeatherData(string city)
        {
            await _liveDataService.UpdateWeather(city);
        }
    }
}
