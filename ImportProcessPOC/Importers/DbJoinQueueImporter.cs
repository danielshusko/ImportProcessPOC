using ImportProcessPOC.Files.Enumerators;
using ImportProcessPOC.Models;

namespace ImportProcessPOC.Importers;

public class DbJoinQueueImporter(ImporterConfig config) : BaseImporter(config)
{
    private const int FileBatchSize = 100;
    private const int ItemBatchSize = 100;

    private Dictionary<int, string> _fileHeaders = [];
    private Dictionary<string, int> _fileHeadersInverted = [];
    private int _lineCount;

    public override string ImporterName => "Join Queue";

    protected override int ProcessFile(string filePath)
    {
        PreProcessFile(filePath);

        ProcessImportLines();

        return _lineCount;
    }

    private void PreProcessFile(string filepath)
    {
        using var preProcessSpan = _metrics.CreateSpan("PreProcessFile");
        _lineCount = 0;

        using var fileEnumerator = new FileEnumerator(filepath, FileBatchSize, _metrics);

        _fileHeaders = fileEnumerator.Headers.ToDictionary(x => x.Key, x => x.Value);
        _fileHeadersInverted = _fileHeaders.ToDictionary(x => x.Value, x => x.Key);
        _repository.SaveImportHeaders(_jobId, fileEnumerator.Headers);

        foreach (var batchOfLines in fileEnumerator)
        {
            _lineCount += batchOfLines.Count;

            _repository.EnqueueImportLines(_jobId, batchOfLines);
        }
    }

    private void ProcessImportLines()
    {
        var processedItems = 0;

        while (processedItems < _lineCount)
        {
            var batchLines = _repository.DequeueImportLines(_jobId, ItemBatchSize);
            var items = batchLines
                        .Select(x => x.Value.Split(','))
                        .Select(
                            x => new Item(
                                TenantId,
                                int.Parse(x[_fileHeadersInverted["Id"]]),
                                int.TryParse(x[_fileHeadersInverted["ParentId"]], out var parentId) ? parentId : null,
                                x[_fileHeadersInverted["Code"]],
                                x[_fileHeadersInverted["Name"]]
                            )
                        )
                        .ToList();
            _repository.SaveItems(items);
            processedItems += items.Count();
        }
    }
}