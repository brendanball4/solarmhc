using Microsoft.EntityFrameworkCore;
using solarmhc.Scraper.Services;
using solarmhc.Scraper;
using solarmhc.Scraper.Data;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<SolarMHCDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddHostedService<Worker>();
        services.AddSingleton<ChromeDriverManager>();
        services.AddSingleton<WebScraperHelperService>();
        services.AddHttpClient<WebScraperService>();
        services.AddScoped<WebScraperService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();
