using Microsoft.EntityFrameworkCore;
using solarmhc.Models.Services;
using solarmhc.Models.Background_Services;
using solarmhc.Models;
using solarmhc.Models.Data;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSingleton<LiveDataService>();
builder.Services.AddSingleton<WebScraperHelperService>();
builder.Services.AddSingleton<EmissionCalculator>();
builder.Services.AddSingleton<EmissionSaved>();
builder.Services.AddSingleton<PowerDataService>();
builder.Services.AddSingleton<WeatherApiService>();
builder.Services.AddSingleton<Graphing>();
builder.Services.AddHostedService<WeatherBackgroundService>();
builder.Services.AddHostedService<DataBackgroundService>();

builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure the DbContext with MySQL connection
builder.Services.AddDbContext<SolarMHCDbContext>(options =>
    options.UseSqlServer(connectionString));

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
