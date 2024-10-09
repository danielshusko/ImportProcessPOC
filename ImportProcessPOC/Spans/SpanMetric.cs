namespace ImportProcessPOC.Spans;

public record SpanMetric(string Name, DateTimeOffset StartDate, TimeSpan RunTime);