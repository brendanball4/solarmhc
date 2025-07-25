﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using solarmhc.Models.Models;
using System.Net;

namespace solarmhc.Models
{
    public class WeatherApiService
    {
        private readonly ILogger<WeatherApiService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherApiService(ILogger<WeatherApiService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            // Retrieve the environment variable (e.g., WEATHER_API) loaded by Docker
            _apiKey = configuration["WEATHER_API"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("API key is not set properly.");
            }
        }

        public async Task<WeatherResponse> GetWeatherAsync(string city)
        {

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("The weather api key is not set properly.");
                return new WeatherResponse
                {
                    Status = false
                };
            }

            var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/forecast.json?key={_apiKey}&q={city}&days=2&aqi=no&alerts=no");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("There was an error grabbing data from the weather service.");
                _logger.LogDebug($"Api key value: {_apiKey}, Status code: {response.StatusCode}");

                return new WeatherResponse { Status = false };
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(content);
        }
    }
}
