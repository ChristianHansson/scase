using Scase.Application.Services;
using Scase.Application.Types;

namespace Scase.Application.UseCases;

public class AllStationsSummarized
{
    private readonly IWeatherService _weatherService;

    public AllStationsSummarized(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// lates-hour default!
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<object> Execute(EWeatherFilterType type, string period, CancellationToken cancellationToken)
    {
        Against.Contains(period, new[] { "latest-hour", "latest-day" }, nameof(period));

        var stationResult = await _weatherService.Stations((int)type, cancellationToken);

        if (stationResult is null)
        {
            // todo: logga!
            return new { };
        }

        var values = await GetAllStationValues((int)type, period, stationResult.Stations, cancellationToken);

        return WeatherSummaryBuilder.BuildSummary(values);
    }

    private async Task<List<decimal>> GetAllStationValues(
        int type,
        string period,
        IReadOnlyList<Station> stations,
        CancellationToken cancellationToken)
    {
        var tasks = stations.Select(async station =>
        {
            var stationInfo = await _weatherService.Station(type, station.Id, period, cancellationToken);

            if (stationInfo is null || stationInfo.Values.Count == 0)
                return (decimal?)null;

            return stationInfo.Values
                .Select(v => decimal.TryParse(v.Value, out var d) ? d : (decimal?)null)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .Sum();
        });

        var results = await Task.WhenAll(tasks);

        return results
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .ToList();
    }
}
