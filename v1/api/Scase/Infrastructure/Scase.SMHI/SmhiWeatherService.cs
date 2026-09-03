using Scase.Application.Services;
using System.Text.Json;

namespace Scase.SMHI;

public class SmhiWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private const int Gust = 1;
    private const int ByTemperatur = 21;

    private const string ByHour = "latest-hour";
    private const string ByDay = "latest-day";
    public SmhiWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }



    // https://opendata-download-metobs.smhi.se/api/version/latest/parameter/21/station/188790/period/latest-hour/data.json

    public async Task<StationsResponse?> Stations(int type, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{type}.json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<StationsResponse>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    // https://opendata-download-metobs.smhi.se/api/version/latest/parameter/1/station/97280/period/latest-day/data.json
    public async Task<StationResponse?> Station(int type, int id, string period, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{type}/station/{id}/period/{period}/data.json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<StationResponse>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
