using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using solarmhc.Models;
using Microsoft.EntityFrameworkCore;
using WebScraper.Data;

namespace solarmhc.Models.Web_Scrapers
{
    internal class WebScraperBackgroundService : BackgroundService
    {
        private readonly ILogger<WebScraperBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _intervalInMinutes = 3;

        public WebScraperBackgroundService(ILogger<WebScraperBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Web scraping service running at: {time}", DateTimeOffset.Now);

                await FroniusFetchAndServeData();
                await APSFetchAndServeData();
                await SunnyFetchAndServeData();
                await HuaweiFetchAndServeData();
                await SolarEdgeFetchAndServeData();

                await Task.Delay(TimeSpan.FromMinutes(_intervalInMinutes), stoppingToken);
            }
        }


        private async Task FroniusFetchAndServeData()
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching and storing data.");
            }
        }
        private async Task APSFetchAndServeData()
        {
            try
            {
                // Data URL
                // Instantiating the service
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Instantiate the DbContext with the scope var
                    // Run the fetch service


                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching and storing data.");
            }
        }
        private async Task SunnyFetchAndServeData()
        {

        }
        private async Task HuaweiFetchAndServeData()
        {

        }
        private async Task SolarEdgeFetchAndServeData()
        {

        }
    }
}
