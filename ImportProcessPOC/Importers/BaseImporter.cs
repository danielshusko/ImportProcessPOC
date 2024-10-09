using System.Diagnostics;
using ImportProcessPOC.Database;
using ImportProcessPOC.Models;
using ImportProcessPOC.Spans;
using UUIDNext;

namespace ImportProcessPOC.Importers;

public abstract class BaseImporter : IImporter
{
    private readonly ImporterConfig _config;
    protected int _jobId;
    protected SpanMetrics _metrics;
    protected ImportRepository _repository;
    protected Guid TenantId { get; }

    protected BaseImporter(ImporterConfig config)
    {
        _config = config;
        TenantId = Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
        _metrics = new SpanMetrics();
        _repository = new ImportRepository(_metrics);
    }

    public abstract string ImporterName { get; }
    public int ItemCount { get; private set; }

    public ImportResult Import(string filePath)
    {
        _jobId = _repository.CreateJob(TenantId, ImporterName, _config.ItemCount, _config.IsOrdered);

        var stopwatch = Stopwatch.StartNew();
        ItemCount = ProcessFile(filePath);
        stopwatch.Stop();

        _repository.EndJob(_jobId);
        _repository.CreateSpans(_jobId);

        return new ImportResult(ImporterName, ItemCount, stopwatch.Elapsed, _metrics);
    }

    protected abstract int ProcessFile(string filePath);
}

public record ImporterConfig(int ItemCount, bool IsOrdered);