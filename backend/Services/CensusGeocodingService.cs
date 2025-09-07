using System.Net.Http;
using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public sealed class CensusGeocodingService(HttpClient http) : IGeocodingService
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private const string Benchmark = "2020"; //2020 is the last full benchmark available

    public async Task<Coordinates?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required", nameof(address));

        // Prepare request URL
        var query = new Dictionary<string, string>
        {
            ["address"] = address,
            ["benchmark"] = Benchmark,
            ["format"] = "json"
        };

        var url = $"geocoder/locations/onelineaddress?{string.Join('&', query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"))}";
        using var resp = await http.GetAsync(url, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        // Navigate JSON: result.addressMatches[0].coordinates.{x,y}
        if (doc.RootElement.GetProperty("result").TryGetProperty("addressMatches", out var addrMatches) &&
            addrMatches.GetArrayLength() > 0)
        {
            var coordsElem = addrMatches[0].GetProperty("coordinates");
            var lon = coordsElem.GetProperty("x").GetDouble();
            var lat = coordsElem.GetProperty("y").GetDouble();
            return new Coordinates { Lat = lat, Lon = lon };
        }

        return null;
    }
}
