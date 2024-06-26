using System;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace solarmhc.Models.Services
{
    public class LiveDataService
    {
        private readonly object _lock = new object();
        private double _utilizationPercentage;
        private decimal _currentWattage;
        private double _totalEmissions;
        private double _savedTrees;
        public event Action OnChange;

        public (double utilizationPercentage, decimal currentWattage) CurrentPower
        {
            get
            {
                lock (_lock)
                {
                    return (_utilizationPercentage, _currentWattage);
                }
            }
            set
            {
                lock (_lock)
                {
                    _utilizationPercentage = value.utilizationPercentage;
                    _currentWattage = value.currentWattage;
                    NotifyStateChanged();
                }
            }
        }

        public double SavedEmissions
        {
            get
            {
                lock (_lock)
                {
                    return _totalEmissions;
                }
            }
            set
            {
                lock (_lock)
                {
                    _totalEmissions = value;
                    NotifyStateChanged();
                }
            }
        }

        public double SavedTrees
        {
            get
            {
                lock (_lock)
                {
                    return _savedTrees;
                }
            }
            set
            {
                lock (_lock)
                {
                    _savedTrees = value;
                    NotifyStateChanged();
                }
            }
        }

        public async Task UpdateCurrentPowerAsync(double utilizationPercentage, decimal currentWattage)
        {
            CurrentPower = (utilizationPercentage, currentWattage);
            await Task.CompletedTask;
        }

        public async Task UpdateCO2Async(double totalEmissions)
        {
            SavedEmissions = totalEmissions;
            await Task.CompletedTask;
        }

        public async Task UpdateTreesAsync(double savedTrees)
        {
            SavedTrees = savedTrees;
            await Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
