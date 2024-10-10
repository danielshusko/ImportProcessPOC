using System.Text;
using ImportProcessPOC.Database.EF;
using ImportProcessPOC.Importers;
using ImportProcessPOC.Models;
using Microsoft.EntityFrameworkCore;

//foreach (var ordered in new[]{true, false})
//{
//    FileGenerator.GenerateFile("../../../Files/", 1, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 500, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 5000, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 10000, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 25000, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 50000, ordered);
//    FileGenerator.GenerateFile(@"../../../Files/", 100000, ordered);
//}

await using var context = new ImportProcessDataContext();
context.Database.Migrate();

var testCount = 1;
List<int> itemCounts = [1, 500, 5000, 25000, 50000, 100000];
List<Func<ImporterConfig, IImporter>> importerFunctions =
[
    config => new HierarchyDepthImporter(config)
    //config => new DbCteImporter(config),
    //config => new DbJoinImporter(config),
    // config => new DbJoinQueueImporter(config),
    // config => new RetryImporter(config)
];
var orders = new[] { "ordered", "random" };

Dictionary<string, ImportResult> results = [];
for (var i = 0; i < testCount; i++)
{
    Console.WriteLine($"Test Count: {i}");
    foreach (var itemCount in itemCounts)
    {
        Console.WriteLine($"  Item Count: {itemCount}");
        foreach (var order in orders)
        {
            Console.WriteLine($"    Order: {order}");
            var filePath = $"../../../Files/items_{itemCount}_{order}.txt";
            foreach (var importerFunc in importerFunctions)
            {
                var config = new ImporterConfig(itemCount, order == "ordered");
                var importer = importerFunc(config);
                Console.WriteLine($"      Importer: {importer.ImporterName}");
                var result = importer.Import(filePath);
                results[$"{importer.ImporterName} - {itemCount} - {order}"] = result;

                WriteResultsToFile();
            }
        }
    }
}

void WriteResultsToFile()
{
    var sbResult = new StringBuilder();
    foreach (var result in results)
    {
        sbResult.AppendLine(
            $"{result.Value.Name}\t{(result.Key.Contains("random") ? "random" : "ordered")}\t{result.Value.ItemCount}\t{result.Value.RunTime.TotalMilliseconds}\t{result.Value.ItemsPerSecond}");
        foreach (var importSubResult in result.Value.Spans)
        {
            sbResult.AppendLine($"\t{importSubResult.Key}\t{importSubResult.TotalRunTimeMs}\t{importSubResult.InstanceCount}\t{importSubResult.AverageRunTimeMs}");
        }
    }

    File.WriteAllText(@"../../../Files/result.txt", sbResult.ToString());
}