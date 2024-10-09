using ImportProcessPOC.Spans;

namespace ImportProcessPOC.Models;

public class ImportSubResults(string key, ICollection<SpanMetric> metrics)
{
    public string Key { get; } = key;
    public int InstanceCount { get; } = metrics.Count;
    public double AverageRunTimeMs { get; } = metrics.Average(x => x.RunTime.TotalMilliseconds);
    public double TotalRunTimeMs { get; } = metrics.Sum(x => x.RunTime.TotalMilliseconds);
}