using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Scraper.Models
{
    public class WeatherResponse
    {
        public Location Location { get; set; }
        public CurrentWeather Current { get; set; }
        public Forecast Forecast { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }

    public class CurrentWeather
    {
        public double Temp_C { get; set; }
        public Condition Condition { get; set; }
        public DateTime Last_Updated { get; set; }
    }

    public class Condition
    {
        public string Text { get; set; }
        public string Icon { get; set; }
    }

    public class Forecast
    {
        public List<ForecastDay> ForecastDay { get; set; }
    }

    public class ForecastDay
    {
        public Day Day { get; set; }
        public List<HourlyForecast> Hour { get; set; }
    }

    public class Day
    {
        public double Maxtemp_C { get; set; }
        public double Mintemp_C { get; set; }
    }

    public class HourlyForecast
    {
        public DateTime Time { get; set; }
        public double Temp_C { get; set; }
        public Condition Condition { get; set; }
    }
}
