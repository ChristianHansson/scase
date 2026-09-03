namespace Scase.Application.UseCases;

internal static class WeatherSummaryBuilder
{
    public static object BuildSummary(List<decimal> values)
    {
        if (values.Count == 0)
        {
            return new
            {
                Count = 0,
                Sum = 0m,
                Mean = 0m,
                Median = 0m,
                Min = 0m,
                Max = 0m,
                StdDev = 0m
            };
        }

        var sorted = values.OrderBy(v => v).ToList();
        var count = sorted.Count;
        var sum = sorted.Sum();
        var mean = sum / count;
        var median = count % 2 == 0
            ? (sorted[count / 2 - 1] + sorted[count / 2]) / 2m
            : sorted[count / 2];
        var min = sorted[0];
        var max = sorted[^1];

        var variance = sorted
            .Select(v => (v - mean) * (v - mean))
            .Sum() / count;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        return new
        {
            Count = count,
            Sum = Math.Round(sum, 1, MidpointRounding.AwayFromZero),
            Mean = Math.Round(mean, 1, MidpointRounding.AwayFromZero),
            Median = Math.Round(median, 1, MidpointRounding.AwayFromZero),
            Min = Math.Round(min, 1, MidpointRounding.AwayFromZero),
            Max = Math.Round(max, 1, MidpointRounding.AwayFromZero),
            StdDev = Math.Round(stdDev, 1, MidpointRounding.AwayFromZero)
        };
    }
}