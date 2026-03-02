using Graphs.Main.Enums;

namespace Graphs.Main.Entities;

internal class DirectedGraph : Graph
{
    public DirectedGraph(IEnumerable<Node> nodes, IEnumerable<Relation> relations) : base(nodes, relations, GraphType.Directed)
    {
    }

    /// <summary>
    /// Creates an adjacency list representation of the directed graph, mapping each node to a list of nodes it has
    /// outgoing edges to.
    /// </summary>
    /// <remarks>Use this method to obtain a structure suitable for efficient traversal or analysis of the
    /// graph's connectivity. The returned adjacency list reflects only the current state of the graph and does not
    /// include nodes without outgoing edges.</remarks>
    /// <returns>A dictionary in which each key is a node in the graph and the corresponding value is a list of nodes that are
    /// directly reachable from that key node. If a node has no outgoing edges, it will not appear as a key in the
    /// dictionary.</returns>
    public override Dictionary<Node, List<Node>> ToAdjacencyList()
    {
        Dictionary<Node, List<Node>> list = new Dictionary<Node, List<Node>>();

        foreach (var node in Nodes)
        {
            list[node] = new List<Node>();
        }

        foreach (var relation in Relations)
        {
            Node from = relation.From;
            Node to = relation.To;

            list[from].Add(to);
        }

        return list;
    }

    /// <summary>
    /// Converts the graph represented by this instance into an adjacency matrix.
    /// </summary>
    /// <remarks>The size of the returned matrix corresponds to the number of nodes in the graph. The matrix
    /// is initialized with zeros, and a value of 1 is set for each directed edge found in the graph's relations. This
    /// method assumes the graph is directed.</remarks>
    /// <returns>A two-dimensional array of integers representing the adjacency matrix of the graph. Each element at position
    /// [i][j] is 1 if there is a directed edge from node i to node j; otherwise, 0.</returns>
    public override int[,] ToAdjacencyMatrix()
    {
        int size = Nodes.Count;
        int[,] matrix = new int[size, size];
        foreach (var relation in Relations)
        {
            int fromIndex = Nodes.IndexOf(relation.From);
            int toIndex = Nodes.IndexOf(relation.To);

            if (fromIndex >= 0 && toIndex >= 0)
            {
                matrix[fromIndex, toIndex] = 1; // Assuming a directed graph
            }
        }

        return matrix;
    }
}
