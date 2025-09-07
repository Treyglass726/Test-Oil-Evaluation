using System.Net;
using System.Text;
using Backend.Models;
using Backend.Services;
using Moq;
using Moq.Protected;

namespace backend.Tests.Services;

public class NwsWeatherServiceTests
{
    [Fact]
    public async Task GetSevenDayForecastAsync_WithValidCoordinates_ShouldReturnForecastWithCorrectTemperatureConversion()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var pointsResponseJson = """
        {
            "properties": {
                "forecast": "https://api.weather.gov/gridpoints/LWX/97,71/forecast"
            }
        }
        """;

        var forecastResponseJson = """
        {
            "properties": {
                "periods": [
                    {
                        "name": "Today",
                        "startTime": "2025-01-06T06:00:00-05:00",
                        "temperature": 45,
                        "temperatureUnit": "F",
                        "shortForecast": "Partly Cloudy",
                        "isDaytime": true
                    },
                    {
                        "name": "Tonight",
                        "startTime": "2025-01-06T18:00:00-05:00",
                        "temperature": 30,
                        "temperatureUnit": "F",
                        "shortForecast": "Clear",
                        "isDaytime": false
                    }
                ]
            }
        }
        """;

        mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsResponseJson, Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(forecastResponseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act
        var result = await service.GetSevenDayForecastAsync(38.8977, -77.0365);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        
        var dayForecast = result[0];
        Assert.Equal(new DateOnly(2025, 1, 6), dayForecast.Date);
        Assert.Equal(7, dayForecast.TemperatureC); // 45F = 7C
        Assert.Equal("Partly Cloudy", dayForecast.Summary);
        Assert.True(dayForecast.IsDaytime);

        var nightForecast = result[1];
        Assert.Equal(new DateOnly(2025, 1, 6), nightForecast.Date);
        Assert.Equal(-1, nightForecast.TemperatureC); // 30F = -1C
        Assert.Equal("Clear", nightForecast.Summary);
        Assert.False(nightForecast.IsDaytime);
    }

    [Fact]
    public async Task GetSevenDayForecastAsync_WithCelsiusTemperature_ShouldReturnUnchangedTemperature()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var pointsResponseJson = """
        {
            "properties": {
                "forecast": "https://api.weather.gov/gridpoints/LWX/97,71/forecast"
            }
        }
        """;

        var forecastResponseJson = """
        {
            "properties": {
                "periods": [
                    {
                        "name": "Today",
                        "startTime": "2025-01-06T06:00:00-05:00",
                        "temperature": 20,
                        "temperatureUnit": "C",
                        "shortForecast": "Sunny",
                        "isDaytime": true
                    }
                ]
            }
        }
        """;

        mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsResponseJson, Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(forecastResponseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act
        var result = await service.GetSevenDayForecastAsync(38.8977, -77.0365);

        // Assert
        Assert.Single(result);
        Assert.Equal(20, result[0].TemperatureC); // Should remain 20C
    }

    [Fact]
    public async Task GetSevenDayForecastAsync_WhenPointsLookupFails_ShouldThrowHttpRequestException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetSevenDayForecastAsync(38.8977, -77.0365));
        Assert.Contains("NWS points lookup failed", exception.Message);
    }

    [Fact]
    public async Task GetSevenDayForecastAsync_WhenForecastUrlMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var pointsResponseJson = """
        {
            "properties": {
                "gridId": "LWX"
            }
        }
        """;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsResponseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetSevenDayForecastAsync(38.8977, -77.0365));
        Assert.Contains("NWS response missing forecast URL", exception.Message);
    }

    [Fact]
    public async Task GetSevenDayForecastAsync_WhenForecastFetchFails_ShouldThrowHttpRequestException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var pointsResponseJson = """
        {
            "properties": {
                "forecast": "https://api.weather.gov/gridpoints/LWX/97,71/forecast"
            }
        }
        """;

        mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pointsResponseJson, Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetSevenDayForecastAsync(38.8977, -77.0365));
        Assert.Contains("NWS forecast fetch failed", exception.Message);
    }

    [Fact]
    public async Task GetSevenDayForecastAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weather.gov/")
        };
        var service = new NwsWeatherService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            service.GetSevenDayForecastAsync(38.8977, -77.0365, CancellationToken.None));
    }
}
