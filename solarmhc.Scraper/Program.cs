using Microsoft.EntityFrameworkCore;
using solarmhc.Scraper;
using solarmhc.Scraper.Data;
using solarmhc.Scraper.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        services.AddDbContext<SolarMHCDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DevSolarMhcDatabase")));


        services.AddHostedService<Worker>();
        services.AddSingleton<ChromeDriverManager>();
        services.AddSingleton<WebScraperHelperService>();
        services.AddHttpClient<WebScraperService>();
        services.AddScoped<WebScraperService>();
    })
    .Build();

await host.RunAsync();
