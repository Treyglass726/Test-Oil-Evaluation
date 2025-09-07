using Backend.Models;

namespace Backend.Services;

public interface IWeatherService
{
    Task<DailyForecast[]> GetSevenDayForecastAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
