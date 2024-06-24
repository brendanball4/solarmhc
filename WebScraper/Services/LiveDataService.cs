using System;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class LiveDataService
    {
        private readonly object _lock = new object();
        private double _utilizationPercentage;
        private decimal _currentWattage;
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
                    OnChange?.Invoke();
                }
            }
        }

        public async Task UpdateCurrentPowerAsync(double utilizationPercentage, decimal currentWattage)
        {
            CurrentPower = (utilizationPercentage, currentWattage);
            await Task.CompletedTask;
        }
    }
}
