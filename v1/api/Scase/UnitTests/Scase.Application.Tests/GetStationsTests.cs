using Moq;
using Scase.Application.DataAccess;
using Scase.Application.Types;
using Scase.Application.UseCases;
using Xunit;
using ServiceStation = Scase.Application.Services.Station;
using ServiceStationsResponse = Scase.Application.Services.StationsResponse;
using IWeatherService = Scase.Application.Services.IWeatherService;

namespace Scase.Application.Tests;

public class GetStationsTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly GenericRepository<EWeatherFilterType, UseCases.StationsResponse> _repository;
    private readonly GetStations _sut;

    public GetStationsTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>(MockBehavior.Strict);
        _repository = new GenericRepository<EWeatherFilterType, UseCases.StationsResponse>();
        _sut = new GetStations(_weatherServiceMock.Object, _repository);
    }

    [Fact]
    public async Task Execute_WhenCacheIsEmpty_CallsWeatherServiceAndReturnsMappedStations()
    {
        // Arrange
        var weatherServiceResponse = new ServiceStationsResponse
        {
            Stations =
            [
                new ServiceStation { Id = 1, Key = "1", Title = "Title1", Name = "Station One" },
                new ServiceStation { Id = 2, Key = "2", Title = "Title2", Name = "Station Two" }
            ]
        };

        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.Gust, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherServiceResponse);

        // Act
        var result = await _sut.Execute(EWeatherFilterType.Gust, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Stations.Count);

        Assert.Equal(1, result.Stations[0].Id);
        Assert.Equal("Station One", result.Stations[0].Name);

        Assert.Equal(2, result.Stations[1].Id);
        Assert.Equal("Station Two", result.Stations[1].Name);

        _weatherServiceMock.Verify(
            s => s.Stations((int)EWeatherFilterType.Gust, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCacheIsEmpty_CachesResultForSubsequentCalls()
    {
        // Arrange
        var weatherServiceResponse = new ServiceStationsResponse
        {
            Stations = [new ServiceStation { Id = 1, Key = "1", Title = "Title1", Name = "Station One" }]
        };

        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherServiceResponse);

        // Act
        var first = await _sut.Execute(EWeatherFilterType.ByTemperature, CancellationToken.None);
        var second = await _sut.Execute(EWeatherFilterType.ByTemperature, CancellationToken.None);

        // Assert
        Assert.Same(first, second);

        _weatherServiceMock.Verify(
            s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCacheAlreadyContainsValue_DoesNotCallWeatherService()
    {
        // Arrange
        var cachedResponse = new UseCases.StationsResponse
        {
            Stations = [new UseCases.StationResponse { Id = 99, Name = "Cached Station" }]
        };

        await _repository.Set(EWeatherFilterType.Gust, cachedResponse, CancellationToken.None);

        // Act
        var result = await _sut.Execute(EWeatherFilterType.Gust, CancellationToken.None);

        // Assert
        Assert.Same(cachedResponse, result);

        _weatherServiceMock.Verify(
            s => s.Stations(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WhenWeatherServiceReturnsNoStations_ReturnsEmptyStationsList()
    {
        // Arrange
        var weatherServiceResponse = new ServiceStationsResponse
        {
            Stations = []
        };

        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.Gust, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherServiceResponse);

        // Act
        var result = await _sut.Execute(EWeatherFilterType.Gust, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Stations);
    }

    [Fact]
    public async Task Execute_PassesCorrectWeatherFilterTypeToWeatherService()
    {
        // Arrange
        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceStationsResponse { Stations = [] });

        // Act
        await _sut.Execute(EWeatherFilterType.ByTemperature, CancellationToken.None);

        // Assert
        _weatherServiceMock.Verify(
            s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_DifferentFilterTypes_AreCachedIndependently()
    {
        // Arrange
        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.Gust, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceStationsResponse
            {
                Stations = [new ServiceStation { Id = 1, Key = "1", Title = "T", Name = "Gust Station" }]
            });

        _weatherServiceMock
            .Setup(s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceStationsResponse
            {
                Stations = [new ServiceStation { Id = 2, Key = "2", Title = "T", Name = "Temp Station" }]
            });

        // Act
        var gustResult = await _sut.Execute(EWeatherFilterType.Gust, CancellationToken.None);
        var tempResult = await _sut.Execute(EWeatherFilterType.ByTemperature, CancellationToken.None);

        // Assert
        Assert.Single(gustResult.Stations);
        Assert.Equal("Gust Station", gustResult.Stations[0].Name);

        Assert.Single(tempResult.Stations);
        Assert.Equal("Temp Station", tempResult.Stations[0].Name);

        _weatherServiceMock.Verify(
            s => s.Stations((int)EWeatherFilterType.Gust, It.IsAny<CancellationToken>()),
            Times.Once);
        _weatherServiceMock.Verify(
            s => s.Stations((int)EWeatherFilterType.ByTemperature, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCancellationTokenIsCancelled_PropagatesToWeatherService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _weatherServiceMock
            .Setup(s => s.Stations(It.IsAny<int>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.Execute(EWeatherFilterType.Gust, cts.Token));
    }
}
