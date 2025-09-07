using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public sealed class NwsWeatherService(HttpClient http) : IWeatherService
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DailyForecast[]> GetSevenDayForecastAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        // Resolve grid forecast URL via /points/{lat},{lon}
        string latStr = latitude.ToString(CultureInfo.InvariantCulture);
        string lonStr = longitude.ToString(CultureInfo.InvariantCulture);
        string pointsPath = $"points/{latStr},{lonStr}";

        using var pointsResp = await http.GetAsync(pointsPath, cancellationToken);
        if (!pointsResp.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"NWS points lookup failed (status {(int)pointsResp.StatusCode}).");
        }

        using var pointsStream = await pointsResp.Content.ReadAsStreamAsync(cancellationToken);
        using var pointsDoc = await JsonDocument.ParseAsync(pointsStream, cancellationToken: cancellationToken);
        if (!pointsDoc.RootElement.TryGetProperty("properties", out var props) ||
            !props.TryGetProperty("forecast", out var forecastUrlElem))
        {
            throw new InvalidOperationException("NWS response missing forecast URL.");
        }

        var forecastUrl = forecastUrlElem.GetString();
        if (string.IsNullOrWhiteSpace(forecastUrl))
            throw new InvalidOperationException("Forecast URL is empty.");

        // Retrieve forecast JSON
        using var forecastResp = await http.GetAsync(forecastUrl, cancellationToken);
        if (!forecastResp.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"NWS forecast fetch failed (status {(int)forecastResp.StatusCode}).");
        }

        using var forecastStream = await forecastResp.Content.ReadAsStreamAsync(cancellationToken);
        using var forecastDoc = await JsonDocument.ParseAsync(forecastStream, cancellationToken: cancellationToken);
        if (!forecastDoc.RootElement.TryGetProperty("properties", out var fProps) ||
            !fProps.TryGetProperty("periods", out var periodsElem))
        {
            throw new InvalidOperationException("NWS forecast JSON missing periods.");
        }

        // Build forecasts: return all periods without filtering
        var result = new List<DailyForecast>();

        foreach (var period in periodsElem.EnumerateArray())
        {
            // Expect properties: name, startTime, temperature, temperatureUnit, shortForecast, isDaytime
            var startTime = period.GetProperty("startTime").GetDateTime();
            var dateOnly = DateOnly.FromDateTime(startTime);
            var isDaytime = period.GetProperty("isDaytime").GetBoolean();

            int temp = period.GetProperty("temperature").GetInt32();
            var tempUnit = period.GetProperty("temperatureUnit").GetString();
            if (tempUnit?.Equals("F", StringComparison.OrdinalIgnoreCase) == true)
            {
                temp = (int)Math.Round((temp - 32) * 5 / 9.0);
            }

            var summary = period.GetProperty("shortForecast").GetString() ?? string.Empty;
            result.Add(new DailyForecast { Date = dateOnly, TemperatureC = temp, Summary = summary, IsDaytime = isDaytime });
        }

        return result.ToArray();
    }
}
