using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using solarmhc.Models.Models;
using solarmhc.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace solarmhc.Models
{
    public class WeatherApiService
    {
        private readonly ILogger<WeatherApiService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherApiService(ILogger<WeatherApiService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable(Constants.WeatherApi.apiKey);
        }

        public async Task<WeatherResponse> GetWeatherAsync(string city)
        {
            var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/forecast.json?key={_apiKey}&q={city}&days=2&aqi=no&alerts=no");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(content);
        }
    }
}
