using Backend.Models;

namespace Backend.Services;

public interface IGeocodingService
{
    Task<Coordinates?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}
