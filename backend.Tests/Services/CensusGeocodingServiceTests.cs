using System.Net;
using System.Text;
using Backend.Models;
using Backend.Services;
using Moq;
using Moq.Protected;

namespace backend.Tests.Services;

public class CensusGeocodingServiceTests
{
    [Fact]
    public async Task GeocodeAsync_WithValidAddress_ShouldReturnCorrectCoordinates()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseJson = """
        {
            "result": {
                "addressMatches": [
                    {
                        "coordinates": {
                            "x": -81.85077,
                            "y": 41.08960
                        }
                    }
                ]
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
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://geocoding.geo.census.gov/")
        };
        var service = new CensusGeocodingService(httpClient);

        // Act
        var result = await service.GeocodeAsync("901 warrington rd Akron oh");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(41.08960, result.Lat);
        Assert.Equal(-81.85077, result.Lon);
    }

    [Fact]
    public async Task GeocodeAsync_WhenNoMatches_ShouldReturnNull()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseJson = """
        {
            "result": {
                "addressMatches": []
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
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://geocoding.geo.census.gov/")
        };
        var service = new CensusGeocodingService(httpClient);

        // Act
        var result = await service.GeocodeAsync("Invalid Address");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GeocodeAsync_WhenHttpError_ShouldReturnNull()
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
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://geocoding.geo.census.gov/")
        };
        var service = new CensusGeocodingService(httpClient);

        // Act
        var result = await service.GeocodeAsync("Some Address");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GeocodeAsync_WithEmptyAddress_ShouldThrowArgumentException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://geocoding.geo.census.gov/")
        };
        var service = new CensusGeocodingService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GeocodeAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GeocodeAsync("   "));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GeocodeAsync(null!));
    }

    [Fact]
    public async Task GeocodeAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
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
            BaseAddress = new Uri("https://geocoding.geo.census.gov/")
        };
        var service = new CensusGeocodingService(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            service.GeocodeAsync("Some Address", CancellationToken.None));
    }
}
