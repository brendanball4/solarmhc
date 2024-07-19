using Newtonsoft.Json.Linq;
using OpenQA.Selenium.DevTools.V123.Network;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace solarmhc.Models.Services
{
    public class LiveDataService
    {
        private readonly ConcurrentDictionary<string, DashboardData> _dashboardData
            = new ConcurrentDictionary<string, DashboardData>();
        private readonly HttpClient _httpClient;
        private WeatherData _weatherData;

        public event Action OnChange;

        public LiveDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public (double utilizationPercentage, decimal currentWattage) GetCurrentPower(string dashboardId)
        {
            if (_dashboardData.TryGetValue(dashboardId, out var data))
            {
                return (data.UtilizationPercentage, data.CurrentWattage);
            }
            return (0, 0);
        }

        public void SetCurrentPower(string dashboardId, decimal currentWattage)
        {
            var data = _dashboardData.GetOrAdd(dashboardId, new DashboardData());
            data.UtilizationPercentage = ((double)currentWattage / 25) * 100;
            data.CurrentWattage = currentWattage;
            NotifyStateChanged();
        }

        public WeatherData GetCurrentWeather()
        {
            return _weatherData;
        }

        public async Task SetCurrentWeather(string city)
        {
            var apiKey = Environment.GetEnvironmentVariable(Constants.WeatherApi.apiKey);
            var response = await _httpClient.GetAsync($"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}&aqi=no");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(content);
            var weatherData = new WeatherData()
            {
                City = jsonData["location"]["name"].Value<string>(),
                Province = jsonData["location"]["region"].Value<string>(),
                Country = jsonData["location"]["country"].Value<string>(),
                Temperature = jsonData["current"]["temp_c"].Value<double>(),
                Condition = jsonData["current"]["condition"]["text"].Value<string>(),
                IconUrl = jsonData["current"]["condition"]["icon"].Value<string>(),
                LastUpdated = jsonData["current"]["last_updated"].Value<DateTime>(),
            };
            _weatherData = weatherData;
            NotifyStateChanged();
        }

        public double GetSavedEmissions(string dashboardId)
        {
            if (_dashboardData.TryGetValue(dashboardId, out var data))
            {
                return data.TotalEmissions;
            }
            return 0;
        }

        public void SetSavedEmissions(string dashboardId, double savedEmissions)
        {
            var data = _dashboardData.GetOrAdd(dashboardId, new DashboardData());
            data.TotalEmissions = savedEmissions;
            NotifyStateChanged();
        }

        public double GetSavedTrees(string dashboardId)
        {
            if (_dashboardData.TryGetValue(dashboardId, out var data))
            {
                return data.SavedTrees;
            }
            return 0;
        }

        public void SetSavedTrees(string dashboardId, double plantedTrees)
        {
            var data = _dashboardData.GetOrAdd(dashboardId, new DashboardData());
            data.SavedTrees = plantedTrees;
            NotifyStateChanged();
        }

        public async Task UpdateCurrentPowerAsync(string dashboardId, decimal currentWattage)
        {
            SetCurrentPower(dashboardId, currentWattage);
            await Task.CompletedTask;
        }

        public async Task UpdateCO2Async(string dashboardId, double totalEmissions)
        {
            SetSavedEmissions(dashboardId, totalEmissions);
            await Task.CompletedTask;
        }

        public async Task UpdateWeather(string city)
        {
            await SetCurrentWeather(city);
            await Task.CompletedTask;
        }

        public async Task UpdateTreesAsync(string dashboardId, double savedTrees)
        {
            SetSavedTrees(dashboardId, savedTrees);
            await Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public class DashboardData
    {
        public double UtilizationPercentage { get; set; }
        public decimal CurrentWattage { get; set; }
        public double TotalEmissions { get; set; }
        public double SavedTrees { get; set; }
    }

    public class WeatherData
    {
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public double Temperature { get; set; }
        public string Condition { get; set; }
        public string IconUrl { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
