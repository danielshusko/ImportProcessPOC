namespace ImportProcessPOC.Spans;

public class SpanMetrics
{
    public List<SpanMetric> Metrics = [];
    private void AddSpanResult(string spanName, TimeSpan runTime) => Metrics.Add(new SpanMetric(spanName, DateTimeOffset.UtcNow, runTime));

    public Span CreateSpan(string name) => new(name, this);

    public class Span(string spanName, SpanMetrics spanMetrics) : IDisposable
    {
        private readonly DateTime _startDate = DateTime.Now;

        public void Dispose()
        {
            var runTime = DateTime.Now - _startDate;
            spanMetrics.AddSpanResult(spanName, runTime);
        }
    }
}