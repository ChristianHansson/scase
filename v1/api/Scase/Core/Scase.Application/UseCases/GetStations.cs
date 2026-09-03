using Scase.Application.DataAccess;
using Scase.Application.Services;
using Scase.Application.Types;

namespace Scase.Application.UseCases;

public class GetStations
{
    private readonly IWeatherService _weatherService;
    private readonly GenericRepository<EWeatherFilterType, StationsResponse> _repository;
    public GetStations(IWeatherService weatherService, GenericRepository<EWeatherFilterType, StationsResponse> repository)
    {
        _weatherService = weatherService;
        _repository = repository;
    }
    public async Task<StationsResponse> Execute(EWeatherFilterType forType, CancellationToken cancellationToken)
    {
        if (await _repository.Get(forType, cancellationToken) is { } cachedResult)
        {
            return cachedResult;
        }

        var stationsResult = await _weatherService.Stations((int)forType, cancellationToken);

        return await _repository.Set(forType, new StationsResponse
        {
            Stations = stationsResult
                .Stations
                .Select(s => new StationResponse
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToList()
        }, cancellationToken);
    }
}

public class StationsResponse
{
    public IReadOnlyList<StationResponse> Stations { get; set; } = [];
}

public class StationResponse
{
    public int Id { get; init; }
    public string Name { get; init; }
}