using System.Text.Json.Serialization;

namespace Scase.Application.Services;

public interface IWeatherService
{
    Task<StationsResponse?> Stations(int type, CancellationToken cancellationToken);
    Task<StationResponse?> Station(int type, int id, string period, CancellationToken cancellationToken);
}


public class StationsResponse
{
    // L0L
    [JsonPropertyName("station")] public IReadOnlyList<Station> Stations { get; set; } = [];
}

public class Station
{
    public int Id { get; init; }
    public string Key { get; init; }
    public string Title { get; init; }
    public string Name { get; init; }
}

public class StationResponse
{
    // L0L
    [JsonPropertyName("value")] public IReadOnlyList<StationValue> Values { get; set; } = [];
}

public class StationValue
{
    // do i need date here since we are using latest-hour or latest-day?
    public string Value { get; init; }
}