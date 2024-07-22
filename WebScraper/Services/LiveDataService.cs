using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.DevTools.V123.Network;
using solarmhc.Models.Models;
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
        private readonly WeatherApiService _weatherApiService;
        private WeatherData _weatherData;

        public event Action OnChange;

        public LiveDataService(WeatherApiService weatherApiService)
        {
            _weatherApiService = weatherApiService;
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
            WeatherResponse weather = await _weatherApiService.GetWeatherAsync(city);
            List<HourlyForecast> forecastHours = new List<HourlyForecast>();

            // I only want to show the next 3 hours of forecast.
            foreach (var time in weather.Forecast.ForecastDay[0].Hour)
            {
                if (time.Time.Hour <= DateTime.Now.Hour || time.Time.Hour > DateTime.Now.AddHours(3).Hour)
                {
                    continue;
                }

                forecastHours.Add(time);
            }

            _weatherData = new WeatherData
            {
                City = weather.Location.Name,
                Province = weather.Location.Region,
                Country = weather.Location.Country,
                Temperature = weather.Current.Temp_C,
                MaxTemperature = weather.Forecast.ForecastDay[0].Day.Maxtemp_C,
                MinTemperature = weather.Forecast.ForecastDay[0].Day.Mintemp_C,
                Condition = weather.Current.Condition.Text,
                IconUrl = weather.Current.Condition.Icon,
                LastUpdated = weather.Current.Last_Updated,
                ForecastData = forecastHours
            };

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
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public string Condition { get; set; }
        public string IconUrl { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<HourlyForecast> ForecastData { get; set; }
    }
}
