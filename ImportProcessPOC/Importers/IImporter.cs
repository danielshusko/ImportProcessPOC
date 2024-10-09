using ImportProcessPOC.Models;

namespace ImportProcessPOC.Importers;

public interface IImporter
{
    public string ImporterName { get; }
    public int ItemCount { get; }
    ImportResult Import(string filePath);
}