namespace Graphs.Main.Entities.Graphs;

internal class UndirectedGraph(IEnumerable<Vertex> nodes, IEnumerable<Edge> edges)
    : Graph(nodes, edges, GraphDirection.Undirected, GraphFeatures.None)
{
    public override Dictionary<Vertex, List<Vertex>> ToAdjacencyList()
    {
        Dictionary<Vertex, List<Vertex>> list = new Dictionary<Vertex, List<Vertex>>();

        foreach (var node in Vertices)
        {
            list[node] = new List<Vertex>();
        }

        foreach (var edge in Edges)
        {
            Vertex from = edge.From;
            Vertex to = edge.To;

            // Add the reverse edge for undirected graph
            list[from].Add(to);
            list[to].Add(from);
        }

        return list;
    }

    public override double?[,] ToAdjacencyMatrix()
    {
        int size = Vertices.Count;
        double?[,] matrix = new double?[size, size];
        foreach (var edge in Edges)
        {
            int fromIndex = Vertices.IndexOf(edge.From);
            int toIndex = Vertices.IndexOf(edge.To);
            if (fromIndex >= 0 && toIndex >= 0)
            {
                matrix[fromIndex, toIndex] = 1;
                matrix[toIndex, fromIndex] = 1;
            }
        }

        return matrix;
    }
}
