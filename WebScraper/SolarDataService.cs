using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solarmhc.Models
{
    public class SolarDataService
    {
        private readonly object _lock = new object();

        public event Action OnChange;

        private string _data;
        public string Data
        {
            get => _data;
            set
            {
                lock (_lock)
                {
                    _data = value;
                    NotifyStateChanged();
                }
            }
        }

        private double[] _chartData = new double[2];
        public double[] ChartData
        {
            get => _chartData;
            set
            {
                lock (_lock)
                {
                    _chartData = value;
                    NotifyStateChanged();
                }
            }
        }

        private double _utilizationPercentage;
        public double UtilizationPercentage
        {
            get => _utilizationPercentage;
            set
            {
                lock (_lock)
                {
                    _utilizationPercentage = value;
                    NotifyStateChanged();
                }
            }
        }

        public void UpdateData(string data, double[] chartData, double utilizationPercentage)
        {
            // Update the data & lock them
            lock (_lock)
            {
                Data = data;
                ChartData = chartData;
                UtilizationPercentage = utilizationPercentage;
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
