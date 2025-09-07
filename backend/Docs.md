# Backend Architecture - Class Diagram

## UML Class Diagram

```mermaid
classDiagram
    class Program {
        +Main(string[] args) void
    }

    class Coordinates {
        +double Lat
        +double Lon
    }

    class DailyForecast {
        +DateOnly Date
        +int TemperatureC
        +string Summary
        +bool IsDaytime
    }

    class IGeocodingService {
        <<interface>>
        +GeocodeAsync(string address, CancellationToken ct) Task~Coordinates?~
    }

    class IWeatherService {
        <<interface>>
        +GetSevenDayForecastAsync(double lat, double lon, CancellationToken ct) Task~DailyForecast[]~
    }

    class CensusGeocodingService {
        -HttpClient http
        -JsonSerializerOptions _jsonOptions
        -string Benchmark
        +GeocodeAsync(string address, CancellationToken ct) Task~Coordinates?~
    }

    class NwsWeatherService {
        -HttpClient http
        -JsonSerializerOptions _jsonOptions
        +GetSevenDayForecastAsync(double lat, double lon, CancellationToken ct) Task~DailyForecast[]~
    }

    %% Relationships
    Program --> IGeocodingService : uses
    Program --> IWeatherService : uses
    Program --> Coordinates : creates
    Program --> DailyForecast : returns

    CensusGeocodingService ..|> IGeocodingService : implements
    NwsWeatherService ..|> IWeatherService : implements

    CensusGeocodingService --> Coordinates : returns
    NwsWeatherService --> DailyForecast : creates
    IGeocodingService --> Coordinates : returns
    IWeatherService --> DailyForecast : returns

    %% Dependencies
    CensusGeocodingService --> HttpClient : depends on
    NwsWeatherService --> HttpClient : depends on
```

### **Models**

#### **Coordinates**
- **Type**: Data model
- **Responsibility**: Represents geographic coordinates
- **Properties**: 
  - `Lat`: Latitude (double)
  - `Lon`: Longitude (double)

#### **DailyForecast**
- **Type**: Data model  
- **Responsibility**: Represents weather forecast for a specific period
- **Properties**:
  - `Date`: Forecast date (DateOnly)
  - `TemperatureC`: Temperature in Celsius (int)
  - `Summary`: Weather description (string)
  - `IsDaytime`: Day/night indicator (bool)

## Dependency Flow

```
HTTP Request
     ↓
Program (API Endpoint)
     ↓
IGeocodingService
     ↓
CensusGeocodingService → Census API
     ↓
Coordinates
     ↓
IWeatherService
     ↓
NwsWeatherService → NWS API
     ↓
DailyForecast[]
     ↓
HTTP Response (JSON)
```

## Testing Architecture

```mermaid
classDiagram
    class CensusGeocodingServiceTests {
        +GeocodeAsync_WithValidAddress_ShouldReturnCorrectCoordinates()
        +GeocodeAsync_WhenNoMatches_ShouldReturnNull()
        +GeocodeAsync_WhenHttpError_ShouldReturnNull()
        +GeocodeAsync_WithEmptyAddress_ShouldThrowArgumentException()
        +GeocodeAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    }

    class NwsWeatherServiceTests {
        +GetSevenDayForecastAsync_WithValidCoordinates_ShouldReturnForecastWithCorrectTemperatureConversion()
        +GetSevenDayForecastAsync_WithCelsiusTemperature_ShouldReturnUnchangedTemperature()
        +GetSevenDayForecastAsync_WhenPointsLookupFails_ShouldThrowHttpRequestException()
        +GetSevenDayForecastAsync_WhenForecastUrlMissing_ShouldThrowInvalidOperationException()
        +GetSevenDayForecastAsync_WhenForecastFetchFails_ShouldThrowHttpRequestException()
        +GetSevenDayForecastAsync_WhenCancellationRequested_ShouldThrowTaskCanceledException()
    }

    class ForecastApiTests {
        +GetForecast_WithValidAddress_ShouldReturnOkWithCorrectForecastData()
        +GetForecast_WhenAddressMissing_ShouldReturnBadRequest()
        +GetForecast_WithEmptyAddress_ShouldReturnBadRequest()
        +GetForecast_WhenAddressNotFound_ShouldReturnNotFound()
        +GetForecast_WhenServiceThrowsException_ShouldReturnInternalServerError()
    }

    CensusGeocodingServiceTests --> CensusGeocodingService : tests
    NwsWeatherServiceTests --> NwsWeatherService : tests
    ForecastApiTests --> Program : tests
```