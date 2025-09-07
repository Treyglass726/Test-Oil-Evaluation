using Backend.Models; // used for data models
using Backend.Services; // used for services
using System.Text.Json; // used for json serialization
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Typed HTTP clients with required User-Agent header (NWS blocks requests without it)
const string userAgent = "Test-Oil-Evaluation/1.0 (+https://example.com)";

builder.Services.AddHttpClient<IGeocodingService, CensusGeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://geocoding.geo.census.gov/");
    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
});

builder.Services.AddHttpClient<IWeatherService, NwsWeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.weather.gov/");
    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // adds swagger ui, in .9 this is not included by default
}

// app.UseHttpsRedirection(); // Disabled for HTTP-only development


app.MapGet("/api/forecast", async (string address, IGeocodingService geocoder, IWeatherService weather, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        logger.LogInformation("Received forecast request for address: {Address}", address);
        
        if (string.IsNullOrWhiteSpace(address))
        {
            logger.LogWarning("Empty address provided");
            return Results.BadRequest("Query parameter 'address' is required.");
        }

        logger.LogInformation("Geocoding address: {Address}", address);
        var coords = await geocoder.GeocodeAsync(address, ct);
        if (coords is null)
        {
            logger.LogWarning("Could not geocode address: {Address}", address);
            return Results.NotFound("Address could not be geocoded.");
        }

        logger.LogInformation("Geocoded to coordinates: {Lat}, {Lon}", coords.Lat, coords.Lon);
        var forecast = await weather.GetSevenDayForecastAsync(coords.Lat, coords.Lon, ct);
        logger.LogInformation("Retrieved forecast with {Count} items", forecast.Length);
        
        return Results.Ok(forecast);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing forecast request for address: {Address}", address);
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("GetForecast")
.WithOpenApi();

app.Run();

