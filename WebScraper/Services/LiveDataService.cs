using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace solarmhc.Models.Services
{
    public class LiveDataService
    {
        private readonly ConcurrentDictionary<string, DashboardData> _dashboardData
            = new ConcurrentDictionary<string, DashboardData>();

        public event Action OnChange;

        public (double utilizationPercentage, decimal currentWattage) GetCurrentPower(string dashboardId)
        {
            if (_dashboardData.TryGetValue(dashboardId, out var data))
            {
                return (data.UtilizationPercentage, data.CurrentWattage);
            }
            return (0, 0);
        }

        public void SetCurrentPower(string dashboardId, double utilizationPercentage, decimal currentWattage)
        {
            var data = _dashboardData.GetOrAdd(dashboardId, new DashboardData());
            data.UtilizationPercentage = utilizationPercentage;
            data.CurrentWattage = currentWattage;
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

        public async Task UpdateCurrentPowerAsync(string dashboardId, double utilizationPercentage, decimal currentWattage)
        {
            SetCurrentPower(dashboardId, utilizationPercentage, currentWattage);
            await Task.CompletedTask;
        }

        public async Task UpdateCO2Async(string dashboardId, double totalEmissions)
        {
            SetSavedEmissions(dashboardId, totalEmissions);
            await Task.CompletedTask;
        }

        public async Task UpdateTreesAsync(string dashboardId, double savedTrees)
        {
            SetSavedTrees(dashboardId, savedTrees);
            await Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    class DashboardData
    {
        public double UtilizationPercentage { get; set; }
        public decimal CurrentWattage { get; set; }
        public double TotalEmissions { get; set; }
        public double SavedTrees { get; set; }
    }
}
