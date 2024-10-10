using ImportProcessPOC.Files.Enumerators;
using ImportProcessPOC.Models;

namespace ImportProcessPOC.Importers;

public class HierarchyDepthImporter(ImporterConfig config) : BaseImporter(config)
{
    private const int FileBatchSize = 100;
    private const int ItemBatchSize = 100;

    private Dictionary<int, string> _fileHeaders = [];
    private Dictionary<string, int> _fileHeadersInverted = [];

    public override string ImporterName => "HierarchyDepth";

    protected override int ProcessFile(string filePath)
    {
        var lineCount = PreProcessFile(filePath);

        ProcessImportLines();

        return lineCount;
    }

    private int PreProcessFile(string filepath)
    {
        using var preProcessSpan = _metrics.CreateSpan("PreProcessFile");
        var lineCount = 0;

        using var fileEnumerator = new FileEnumerator(filepath, FileBatchSize, _metrics);

        _fileHeaders = fileEnumerator.Headers.ToDictionary(x => x.Key, x => x.Value);
        _fileHeadersInverted = _fileHeaders.ToDictionary(x => x.Value, x => x.Key);
        _repository.SaveImportHeaders(_jobId, fileEnumerator.Headers);

        foreach (var batchOfLines in fileEnumerator)
        {
            lineCount += batchOfLines.Count;

            _repository.SaveImportLines(_jobId, batchOfLines);
        }

        _repository.SetLineHierarchyDepths(_jobId);

        return lineCount;
    }

    private void ProcessImportLines()
    {
        foreach (var lineBatch in _repository.GetLinesByDepth(_jobId, ItemBatchSize))
        {
            var items = lineBatch
                        .Select(x => x.Item2.Split(','))
                        .Select(
                            x => new Item(
                                TenantId,
                                int.Parse(x[_fileHeadersInverted["Id"]]),
                                int.TryParse(x[_fileHeadersInverted["ParentId"]], out var parentId) ? parentId : null,
                                x[_fileHeadersInverted["Code"]],
                                x[_fileHeadersInverted["Name"]]
                            )
                        );
            _repository.SaveItems(items.ToList());
        }
    }
}