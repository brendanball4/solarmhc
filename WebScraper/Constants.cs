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
        }

        public static class DataUrls
        {
            public const string SolarEdge = "https://monitoring.solaredge.com/solaredge-web/p/login";
            public const string Sunny = "https://ennexos.sunnyportal.com/login?next=%2Fdashboard%2Finitialize";
            public const string APS = "https://apsystemsema.com/ema/logoutEMA.action";
            public const string Huawei = "https://huawei.frankensolar.ca:8443/index.action?ver=1700150331378";
            public const string Fronius = "https://www.solarweb.com/Home/GuestLogOn?pvSystemId=fadf7bd8-f901-4948-8683-a4dfd7d8b677";
        }

        public static class Capacities
        {
            public const double SolarEdge = 20;
            public const double Sunny = 25;
            public const double APS = 25;
            public const double Huawei = 25;
            public const double Fronius = 25;
        }

        public static class TargetedElements
        {
            public const string SolarEdge = "SolarEdge";
            public const string Sunny = "Sunny";
            public const string APS = "APS";
            public const string Huawei = "Huawei";
            public const string Fronius = "div.js-status-bar-text";
        }
    }
}
