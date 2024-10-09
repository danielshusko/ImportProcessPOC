namespace ImportProcessPOC.Files.FileGenerators;

public static class FileGenerator
{
    private static readonly Random Rand = new();

    public static string GenerateFile(string path, int itemCount, bool ordered)
    {
        var fileName = Path.Combine(path, $"items_{itemCount}_{(ordered ? "ordered" : "random")}.txt");
        File.AppendAllLines(fileName, ["Id,ParentId,Code,Name"]);

        var graph = new Graph();
        for (var id = 1; id < itemCount + 1; id++)
        {
            graph.AddNode(id);

            if (!(Rand.NextDouble() > 0.5))
            {
                continue;
            }

            for (var retry = 0; retry < 3; retry++)
            {
                var parent = ordered
                                 ? Rand.Next(1, id)
                                 : Rand.Next(1, itemCount + 1);
                graph.AddNode(parent);
                if (!graph.IsValidParent(id, parent))
                {
                    continue;
                }

                graph.SetParent(id, parent);
                break;
            }
        }

        File.AppendAllLines(
            fileName,
            graph.Nodes.Values
                 .Select(x => $"{x.Id},{x.Parent?.Id.ToString() ?? string.Empty},item {x.Id},item name {x.Id}")
        );

        return fileName;
    }

    public class Graph
    {
        public readonly SortedDictionary<int, GraphNode> Nodes = new();

        public void AddNode(int id)
        {
            if (!Nodes.ContainsKey(id))
            {
                Nodes.Add(id, new GraphNode(id));
            }
        }

        public void SetParent(int id, int parentId)
        {
            var node = Nodes[id];
            var parentNode = Nodes[parentId];
            node.SetParent(parentNode);
        }

        public bool IsValidParent(int id, int parentId)
        {
            var parentNode = Nodes[parentId];
            var parentHierarchy = parentNode.GetHierarchy();
            return !parentHierarchy.Contains(id);
        }

        public class GraphNode(int id)
        {
            public int Id { get; } = id;
            public GraphNode? Parent { get; set; }

            public List<int> GetHierarchy()
            {
                var hierarchy = Parent == null
                                    ? []
                                    : Parent.GetHierarchy();
                hierarchy.Add(Id);

                return hierarchy;
            }

            public void SetParent(GraphNode parent) => Parent = parent;
        }
    }
}