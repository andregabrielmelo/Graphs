using Graphs.Main.Enums;

namespace Graphs.Main.Entities;

internal abstract class Graph
{
    public List<Node> Nodes { get; private set; }
    public List<Relation> Relations { get; private set; }
    public GraphType Type { get; private set; }

    public Graph(IEnumerable<Node> nodes, IEnumerable<Relation> relations, GraphType type)
    {
        Nodes = new List<Node>(nodes);
        Relations = new List<Relation>(relations);
        Type = type;
    }

    public abstract int[,] ToAdjacencyMatrix();

    public abstract Dictionary<Node, List<Node>> ToAdjacencyList();
}
