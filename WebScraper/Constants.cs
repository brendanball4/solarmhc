using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models
{
    public static class Constants
    {
        public static class Names
        {
            public const string SolarEdge = "SolarEdge";
            public const string Sunny = "Sunny";
            public const string APS = "APS";
            public const string Huawei = "Huawei";
            public const string Fronius = "Fronius";
            public const string Total = "Total Power";
        }

        public static class Capacities
        {
            public const double SolarEdge = 20;
            public const double Sunny = 25;
            public const double APS = 25;
            public const double Huawei = 25;
            public const double Fronius = 25;
            public const double TotalCapacity = 120;
        }

        public static class WeatherApi
        {
            public const string apiKey = "WEATHER_API";
        }

        public static class Environmental
        {
            public static class Canada
            {
                public const decimal CO2Factor = 392;
                public const decimal Trees = 0.0117M;
            }
        }
    }
}
