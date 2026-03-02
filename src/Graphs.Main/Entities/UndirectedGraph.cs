using Graphs.Main.Enums;

namespace Graphs.Main.Entities;

internal class UndirectedGraph : Graph
{
    public UndirectedGraph(IEnumerable<Node> nodes, IEnumerable<Relation> relations) : base(nodes, relations, GraphType.Undirected)
    {
    }

    /// <summary>
    /// Converts the undirected graph represented by this instance into an adjacency list.
    /// </summary>
    /// <remarks>The adjacency list reflects an undirected graph structure, so each connection between two
    /// nodes is represented in both directions. The returned dictionary includes all nodes in the graph, even those
    /// without any connections.</remarks>
    /// <returns>A dictionary in which each key is a node and the corresponding value is a list of nodes directly connected to
    /// that node.</returns>
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

            // Add the reverse relation for undirected graph
            list[from].Add(to);
            list[to].Add(from);
        }

        return list;
    }

    /// <summary>
    /// Converts the undirected graph to its adjacency matrix representation.
    /// </summary>
    /// <remarks>The returned adjacency matrix is symmetric, reflecting the undirected nature of the graph.
    /// The size of the matrix corresponds to the number of nodes in the graph, with each row and column representing a
    /// node.</remarks>
    /// <returns>A two-dimensional jagged array of integers where each element indicates the presence of an edge between nodes. A
    /// value of 1 represents an edge; otherwise, 0.</returns>
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
                matrix[fromIndex, toIndex] = 1; // Mark the edge from 'from' to 'to'
                matrix[toIndex, fromIndex] = 1; // Mark the edge from 'to' to 'from' for undirected graph
            }
        }

        return matrix;
    }
}
