using System.Collections;
using ImportProcessPOC.Models;
using ImportProcessPOC.Spans;

namespace ImportProcessPOC.Files.Enumerators;

public class FileEnumerator : IEnumerable<List<FileLine>>, IEnumerator<List<FileLine>>
{
    private readonly int _batchSize;

    private readonly StreamReader _fileReader;
    private readonly SpanMetrics _metrics;

    private int _headerIdIndex;
    private int _headerParentIdIndex;
    private int _lineIndex = 1;
    public Dictionary<int, string> Headers { get; private set; } = [];

    public FileEnumerator(string filePath, int batchSize, SpanMetrics metrics)
    {
        _batchSize = batchSize;
        _metrics = metrics;
        _fileReader = new StreamReader(filePath);

        SetHeaders();
    }

    public IEnumerator<List<FileLine>> GetEnumerator() => this;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool MoveNext()
    {
        if (_fileReader.EndOfStream)
        {
            return false;
        }

        Current = [];

        using var span = _metrics.CreateSpan("FileEnumerator.MoveNext");
        while (!_fileReader.EndOfStream && Current.Count < _batchSize)
        {
            var line = _fileReader.ReadLine()!;

            var lineTokens = line.Split(',');
            var id = int.Parse(lineTokens[_headerIdIndex]);
            var parentId = int.TryParse(lineTokens[_headerParentIdIndex], out var tempParentId) ? tempParentId : (int?) null;
            Current.Add(new FileLine(_lineIndex, id, parentId, line));
            _lineIndex++;
        }

        return true;
    }

    public void Reset() => throw new NotImplementedException();

    public List<FileLine> Current { get; private set; } = [];

    object IEnumerator.Current => Current;

    public void Dispose() => _fileReader.Dispose();

    private void SetHeaders()
    {
        var line = _fileReader.ReadLine()!;
        Headers = line.Split(',')
                      .Select((x, i) => new { Index = i, Header = x })
                      .ToDictionary(x => x.Index, x => x.Header);

        _headerIdIndex = Headers.First(x => x.Value == "Id").Key;
        _headerParentIdIndex = Headers.First(x => x.Value == "ParentId").Key;
    }
}