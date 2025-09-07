using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace backend.Tests.Integration;

public class ForecastApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ForecastApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetForecast_WithValidAddress_ShouldReturnOkWithCorrectForecastData()
    {
        // Arrange
        var mockGeocodingService = new Mock<IGeocodingService>();
        var mockWeatherService = new Mock<IWeatherService>();

        var coordinates = new Coordinates { Lat = 38.8977, Lon = -77.0365 };
        var forecast = new DailyForecast[]
        {
            new() { Date = DateOnly.FromDateTime(DateTime.Today), TemperatureC = 20, Summary = "Sunny", IsDaytime = true },
            new() { Date = DateOnly.FromDateTime(DateTime.Today), TemperatureC = 10, Summary = "Clear", IsDaytime = false }
        };

        mockGeocodingService
            .Setup(x => x.GeocodeAsync("1600 Pennsylvania Ave", It.IsAny<CancellationToken>()))
            .ReturnsAsync(coordinates);

        mockWeatherService
            .Setup(x => x.GetSevenDayForecastAsync(38.8977, -77.0365, It.IsAny<CancellationToken>()))
            .ReturnsAsync(forecast);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real services
                var geocodingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGeocodingService));
                if (geocodingDescriptor != null)
                    services.Remove(geocodingDescriptor);

                var weatherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWeatherService));
                if (weatherDescriptor != null)
                    services.Remove(weatherDescriptor);

                // Add mocked services
                services.AddSingleton(mockGeocodingService.Object);
                services.AddSingleton(mockWeatherService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/forecast?address=1600 Pennsylvania Ave");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DailyForecast[]>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("Sunny", result[0].Summary);
        Assert.True(result[0].IsDaytime);
        Assert.Equal("Clear", result[1].Summary);
        Assert.False(result[1].IsDaytime);
    }

    [Fact]
    public async Task GetForecast_WhenAddressMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/forecast");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_WithEmptyAddress_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/forecast?address=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_WhenAddressNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var mockGeocodingService = new Mock<IGeocodingService>();
        var mockWeatherService = new Mock<IWeatherService>();

        mockGeocodingService
            .Setup(x => x.GeocodeAsync("Invalid Address", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coordinates?)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real services
                var geocodingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGeocodingService));
                if (geocodingDescriptor != null)
                    services.Remove(geocodingDescriptor);

                var weatherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWeatherService));
                if (weatherDescriptor != null)
                    services.Remove(weatherDescriptor);

                // Add mocked services
                services.AddSingleton(mockGeocodingService.Object);
                services.AddSingleton(mockWeatherService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/forecast?address=Invalid Address");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var mockGeocodingService = new Mock<IGeocodingService>();
        var mockWeatherService = new Mock<IWeatherService>();

        mockGeocodingService
            .Setup(x => x.GeocodeAsync("Test Address", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real services
                var geocodingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGeocodingService));
                if (geocodingDescriptor != null)
                    services.Remove(geocodingDescriptor);

                var weatherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWeatherService));
                if (weatherDescriptor != null)
                    services.Remove(weatherDescriptor);

                // Add mocked services
                services.AddSingleton(mockGeocodingService.Object);
                services.AddSingleton(mockWeatherService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/forecast?address=Test Address");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}

public class FakeGeocodingService : IGeocodingService
{
    public Task<Coordinates?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        return address switch
        {
            "1600 Pennsylvania Ave NW" => Task.FromResult<Coordinates?>(new Coordinates { Lat = 38.8977, Lon = -77.0365 }),
            "Valid Address" => Task.FromResult<Coordinates?>(new Coordinates { Lat = 40.7128, Lon = -74.0060 }),
            _ => Task.FromResult<Coordinates?>(null)
        };
    }
}

public class FakeWeatherService : IWeatherService
{
    public Task<DailyForecast[]> GetSevenDayForecastAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var forecast = new DailyForecast[]
        {
            new() { Date = DateOnly.FromDateTime(DateTime.Today), TemperatureC = 22, Summary = "Sunny", IsDaytime = true },
            new() { Date = DateOnly.FromDateTime(DateTime.Today), TemperatureC = 15, Summary = "Clear", IsDaytime = false },
            new() { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), TemperatureC = 25, Summary = "Partly Cloudy", IsDaytime = true },
            new() { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), TemperatureC = 18, Summary = "Mostly Clear", IsDaytime = false }
        };

        return Task.FromResult(forecast);
    }
}
