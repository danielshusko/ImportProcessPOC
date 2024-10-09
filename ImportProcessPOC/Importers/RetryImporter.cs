using ImportProcessPOC.Files.Enumerators;
using ImportProcessPOC.Models;

namespace ImportProcessPOC.Importers;

public class RetryImporter(ImporterConfig config) : BaseImporter(config)
{
    private const int FileBatchSize = 25;
    private readonly LineTracker _lineTracker = new();

    public override string ImporterName => "Retry";

    protected override int ProcessFile(string filePath)
    {
        var lineCount = ProcessFileInternal(filePath);
        while (lineCount.LinesProcessed < lineCount.TotalLines)
        {
            var retryCount = ProcessFileInternal(filePath);
            lineCount = lineCount with { LinesProcessed = lineCount.LinesProcessed + retryCount.LinesProcessed };
        }

        return lineCount.TotalLines;
    }

    private FileLines ProcessFileInternal(string filePath)
    {
        using var span = _metrics.CreateSpan("ProcessFile");

        var linesProcessed = 0;
        var totalLines = 0;

        using var fileEnumerator = new FileEnumerator(filePath, FileBatchSize, _metrics);
        var headers = fileEnumerator.Headers.ToDictionary(x => x.Value, x => x.Key);

        foreach (var batchOfLines in fileEnumerator)
        {
            totalLines += batchOfLines.Count;
            var items = batchOfLines
                        .Where(x => !_lineTracker.IsLineCovered(x.Index))
                        .Select(
                            x => new
                                 {
                                     LineIndex = x.Index,
                                     Tokens = x.Line.Split(',')
                                 })
                        .Select(
                            x => new
                                 {
                                     x.LineIndex,
                                     Item = new Item(
                                         TenantId,
                                         int.Parse(x.Tokens[headers["Id"]]),
                                         int.TryParse(x.Tokens[headers["ParentId"]], out var parentId) ? parentId : null,
                                         x.Tokens[headers["Code"]],
                                         x.Tokens[headers["Name"]]
                                     )
                                 })
                        .ToList();
            try
            {
                _repository.SaveItems(items.Select(x => x.Item).ToList());
                linesProcessed += batchOfLines.Count;
                _lineTracker.AddLines(batchOfLines.First().Index, batchOfLines.Last().Index);
            }
            catch (Exception)
            {
                _repository.Reset();
                foreach (var item in items)
                {
                    try
                    {
                        _repository.SaveItems([item.Item]);
                        linesProcessed += 1;
                        _lineTracker.AddLines(item.LineIndex, item.LineIndex);
                    }
                    catch (Exception)
                    {
                        _repository.Reset();
                    }
                }
            }
        }

        return new FileLines(totalLines, linesProcessed);
    }

    private class LineTracker
    {
        private List<Lines> _lines = [];

        public void AddLines(int start, int end)
        {
            _lines.Add(new Lines(start, end));
            Rebalance();
        }

        public bool IsLineCovered(int lineNumber) => _lines.Any(x => lineNumber >= x.Start && lineNumber <= x.End);

        private void Rebalance()
        {
            var sortedLines = _lines.OrderBy(x => x.Start).ToList();
            _lines = [];

            var currentStart = sortedLines[0].Start;
            var currentEnd = sortedLines[0].End;
            foreach (var line in sortedLines.Skip(1))
            {
                if (line.Start == currentEnd + 1)
                {
                    currentEnd = line.End;
                }
                else
                {
                    _lines.Add(new Lines(currentStart, currentEnd));
                    currentStart = line.Start;
                    currentEnd = line.End;
                }
            }

            _lines.Add(new Lines(currentStart, currentEnd));
        }

        private record Lines(int Start, int End);
    }

    private record FileLines(int TotalLines, int LinesProcessed);
}