using Microsoft.VisualBasic;
using solarmhc.Models.Models;
using System.Collections.Concurrent;

namespace solarmhc.Models.Services
{
    public class LiveDataService
    {
        private readonly ConcurrentDictionary<string, DashboardData> _dashboardData
            = new ConcurrentDictionary<string, DashboardData>();
        private readonly ConcurrentDictionary<string, List<PowerData>> _powerData
            = new ConcurrentDictionary<string, List<PowerData>>();
        private readonly WeatherApiService _weatherApiService;
        private readonly PowerDataService _powerDataService;
        private WeatherData _weatherData;

        public event Action OnChange;

        public LiveDataService(WeatherApiService weatherApiService, PowerDataService powerDataService)
        {
            _weatherApiService = weatherApiService;
            _powerDataService = powerDataService;
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

            int currentHour = DateTime.Now.Hour;
            int hoursToShow = 3;
            int endOfDayHour = 23;

            if (currentHour + hoursToShow > endOfDayHour)
            {
                // Handle transition to the next day
                int remainingHoursToday = endOfDayHour - currentHour;
                int hoursFromNextDay = hoursToShow - remainingHoursToday;

                for (int i = currentHour + 1; i <= endOfDayHour; i++)
                {
                    forecastHours.Add(weather.Forecast.ForecastDay[0].Hour[i]);
                }

                for (int i = 0; i < hoursFromNextDay; i++)
                {
                    forecastHours.Add(weather.Forecast.ForecastDay[1].Hour[i]);
                }
            }
            else
            {
                // Only fetch hours from the current day
                for (int i = currentHour + 1; i < currentHour + hoursToShow; i++)
                {
                    forecastHours.Add(weather.Forecast.ForecastDay[0].Hour[i]);
                }
            }

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

        public async Task<List<PowerData>> GetPowerData(string dashboardId)
        {
            if (_powerData.TryGetValue(dashboardId, out var data))
            {
                return data;
            }
            return new List<PowerData>();
        }

        public async Task SetPowerData(string dashboardId)
        {
            _powerData.GetOrAdd(dashboardId, await _powerDataService.GetPowerDataForDateAsync(dashboardId));
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

        public async Task UpdatePowerDataAsync(string dashboardId)
        {
            await SetPowerData(dashboardId);
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
