namespace Graphs.Main.Entities.Graphs;

internal class DirectedGraph(IEnumerable<Vertex> vertexs, IEnumerable<Edge> edges)
    : Graph(vertexs, edges, GraphDirection.Directed, GraphFeatures.None)
{
    public override Dictionary<Vertex, List<Vertex>> ToAdjacencyList()
    {
        Dictionary<Vertex, List<Vertex>> list = new Dictionary<Vertex, List<Vertex>>();

        foreach (var vertex in Vertices)
        {
            list[vertex] = new List<Vertex>();
        }

        foreach (var edge in Edges)
        {
            Vertex from = edge.From;
            Vertex to = edge.To;

            list[from].Add(to);
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
                matrix[fromIndex, toIndex] = 1; // Assuming a directed graph
            }
        }

        return matrix;
    }
}
