using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using solarmhc.Scraper.Services;
using solarmhc.Scraper;
using solarmhc.Scraper.Data;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create and configure the host
        IHost host = Host.CreateDefaultBuilder(args)
            .UseWindowsService() // If running as a Windows Service
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Add environment variables as a configuration source
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                string connString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection", EnvironmentVariableTarget.Machine);

                // Configure the DbContext with a connection string from environment variables
                services.AddDbContext<SolarMHCDbContext>(options =>
                    options.UseSqlServer(connString));

                // Register hosted services
                services.AddHostedService<Worker>();

                // Register singleton services
                services.AddSingleton<ChromeDriverManager>();
                services.AddSingleton<WebScraperHelperService>();

                // Register HttpClient and scoped services
                services.AddHttpClient<WebScraperService>();
                services.AddScoped<WebScraperService>();
            })
            .ConfigureLogging(logging =>
            {
                // Configure logging to console
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        // Run the host asynchronously
        await host.RunAsync();
    }
}
