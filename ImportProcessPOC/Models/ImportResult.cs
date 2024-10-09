using ImportProcessPOC.Spans;

namespace ImportProcessPOC.Models;

public record ImportResult(string Name, int ItemCount, TimeSpan RunTime, SpanMetrics SpanMetrics)
{
    public double ItemsPerSecond { get; } = ItemCount / RunTime.TotalSeconds;

    public List<ImportSubResults> Spans { get; } = SpanMetrics.Metrics
                                                              .GroupBy(x => x.Name)
                                                              .Select(x => new ImportSubResults(x.Key, x.ToList()))
                                                              .ToList();
}