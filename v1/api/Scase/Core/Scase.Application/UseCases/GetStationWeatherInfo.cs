using Scase.Application.Services;
using Scase.Application.Types;

namespace Scase.Application.UseCases;

public class GetStationWeatherInfo
{
    private readonly IWeatherService _weatherService;

    public GetStationWeatherInfo(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    // AAAJ jag saknar min resharper :D :D 
    public async Task<object> Execute(EWeatherFilterType type, string station, string period, CancellationToken cancellationToken)
    {
        if (!int.TryParse(station, out var stationId))
        {
            // todo: logga!
            return new { };
        }
        Against.Contains(period, new[] { "latest-hour", "latest-day" }, nameof(period));

        var stationInfo = await _weatherService.Station((int)type, stationId, period, cancellationToken);

        if (stationInfo is null || stationInfo.Values.Count == 0)
        {
            // todo: logga!
            return new { };
        }

        var values = stationInfo.Values
            .Select(v => decimal.TryParse(v.Value, out var d) ? d : (decimal?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();

        return WeatherSummaryBuilder.BuildSummary(values);
    }
}
