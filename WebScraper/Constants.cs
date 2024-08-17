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
            public const string Total = "Total Intake";
        }

        public static class EnvironmentVars
        {
            public static class EnvironmentNames
            {
                public const string Sunny = "MY_APP_SUNNY_USERNAME";
                public const string Huawei = "MY_APP_HUA_USERNAME";
                public const string APS = "MY_APP_APS_USERNAME";
                public const string SolarEdgeApi = "SE_API";
                public const string SolarEdgeSite = "SE_SITE";
            }
            public static class EnvironmentPass
            {
                public const string Sunny = "MY_APP_SUNNY_PASSWORD";
                public const string Huawei = "MY_APP_HUA_PASSWORD";
                public const string APS = "MY_APP_APS_PASSWORD";
            }
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
            public const double TotalCapacity = 120;
        }

        public static class WeatherApi
        {
            public const string apiKey = "WEATHER_API";
        }

        public static class TargetedElements
        {
            public static class Fronius
            {
                public static class Data
                {
                    public const string kwId = "div.js-status-bar-text";
                }
            }

            public static class APS
            {
                public static class Data
                {
                    public const string kwId = "a#today";
                }

                public static class Auth
                {
                    public const string loginForm = "loginForm";
                    public const string username = "username";
                    public const string password = "password";
                    public const string loginButton = "input#Login";
                }
            }

            public static class Huawei
            {
                public static class Data
                {
                    public const string kwId = "span#pvSystemOverviewPower";
                }

                public static class Auth
                {
                    public const string loginForm = "loginForm";
                    public const string username = "userName";
                    public const string password = "password";
                    public const string loginButton = "a[href='javascript:void(0)']";
                }
            }

            public static class Sunny
            {
                public static class Data
                {
                    public const string kwId = "span.sma-value-label";
                }

                public static class Auth
                {
                    public const string cookiePopup = "button#onetrust-reject-all-handler";
                    public const string loginForm = "[data-testid='login']";
                    public const string username = "mat-input-0";
                    public const string password = "mat-input-1";
                    public const string loginButton = "[data-testid='button-primary']";
                }
            }
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
