using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using WebScraper.Data;
using solarmhc.Models.Services;
using solarmhc.Models.Services.Web_Scrapers;
using solarmhc.Models.Background_Services;
using solarmhc.Models;
using OpenQA.Selenium.Chrome;
using ApexCharts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient<DataWebScraper>();
builder.Services.AddMudServices();
builder.Services.AddSingleton<LiveDataService>();
builder.Services.AddSingleton<WebScraperHelperService>();
builder.Services.AddSingleton<EmissionCalculator>();
builder.Services.AddSingleton<EmissionSaved>();
builder.Services.AddHostedService<DataWebScraperBackgroundService>();

builder.Services.AddScoped(sp => ChromeDriverFactory.CreateChromeDriver());
builder.Services.AddScoped<DataWebScraper>();

// Configure DbContext
builder.Services.AddDbContext<SolarMHCDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SolarMhcDatabase")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
