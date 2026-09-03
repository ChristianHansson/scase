using Microsoft.AspNetCore.Mvc;
using Scase.Application.DataAccess;
using Scase.Application.Services;
using Scase.Application.Types;
using Scase.Application.UseCases;
using Scase.Authentication;
using Scase.SMHI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services
    .AddHttpClient<IWeatherService, SmhiWeatherService>(client =>
    {
        client.BaseAddress = new Uri("https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/", UriKind.RelativeOrAbsolute);
    });

builder.Services.AddScoped<GetStations>();
builder.Services.AddScoped<GetStationWeatherInfo>();
builder.Services.AddScoped<AllStationsSummarized>();
builder.Services.AddSingleton<GenericRepository<EWeatherFilterType, Scase.Application.UseCases.StationsResponse>>();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationDefaults.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.SchemeName,
        options => options.ApiKey = builder.Configuration["ApiKey"] ?? string.Empty);
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/weather/{type}", async ([FromRoute] string type,
    [FromServices] AllStationsSummarized useCase,
    CancellationToken cancellationToken,
    [FromQuery] string? period = "latest-hour") =>
{
    if (!Enum.TryParse<EWeatherFilterType>(type, true, out var filterType))
    {
        return Results.BadRequest();
    }
    return Results.Ok(await useCase.Execute(filterType, period, cancellationToken));
}).RequireAuthorization();

app.MapGet("/weather/{type}/stations", async ([FromRoute] string type,
    [FromServices] GetStations useCase,
    CancellationToken cancellationToken) =>
{
    if (!Enum.TryParse<EWeatherFilterType>(type, true, out var filterType))
    {
        return Results.BadRequest();
    }
    return Results.Ok(await useCase.Execute(filterType, cancellationToken));
}).RequireAuthorization();

app.MapGet("/weather/{type}/stations/{station}", async ([FromRoute] string type,
    [FromRoute] string station,
    [FromServices] GetStationWeatherInfo useCase,
    CancellationToken cancellationToken,
    [FromQuery] string? period = "latest-hour") =>
{
    if (!Enum.TryParse<EWeatherFilterType>(type, true, out var filterType))
    {
        return Results.BadRequest();
    }
    return Results.Ok(await useCase.Execute(filterType, station, period, cancellationToken));
}).RequireAuthorization();

app.Run();
