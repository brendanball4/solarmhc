using Microsoft.EntityFrameworkCore;
using solarmhc.Scraper;
using solarmhc.Scraper.Data;
using solarmhc.Scraper.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Access the environment from context.HostingEnvironment
        var isDevelopment = context.HostingEnvironment.IsDevelopment();

        // Configure DbContext with the appropriate connection string
        if (!isDevelopment)
        {
            services.AddDbContext<SolarMHCDbContext>(options =>
                options.UseSqlServer(context.Configuration.GetConnectionString("DevSolarMhcDatabaseDevelopment")));
        }
        else
        {
            var username = Environment.GetEnvironmentVariable("SQLSERVER_USERNAME");
            var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD");

            
            var connectionString = $"Server=DESKTOP-CVG4ADF\\SQLEXPRESS; User ID={username};Password={password};Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            services.AddDbContext<SolarMHCDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddHostedService<Worker>();
        services.AddSingleton<ChromeDriverManager>();
        services.AddSingleton<WebScraperHelperService>();
        services.AddHttpClient<WebScraperService>();
        services.AddScoped<WebScraperService>();
    })
    .Build();

await host.RunAsync();
