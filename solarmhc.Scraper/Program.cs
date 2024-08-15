using Microsoft.EntityFrameworkCore;
using solarmhc.Scraper;
using solarmhc.Scraper.Data;
using solarmhc.Scraper.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Access the configuration from context.Configuration
        services.AddHostedService<Worker>();
        services.AddSingleton<ChromeDriverManager>();
        services.AddSingleton<WebScraperHelperService>();
        services.AddHttpClient<WebScraperService>();
        services.AddScoped<WebScraperService>();

        // Configure DbContext with the connection string
        services.AddDbContext<SolarMHCDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("SolarMhcDatabase")));
    })
    .Build();

await host.RunAsync();
