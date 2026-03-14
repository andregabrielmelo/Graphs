using Graphs.Main.Entities.Graphs;

namespace Graphs.Main.Entities.Algorithms;

internal class StronglyConnectedComponentsAlgorithm : Algorithm<Graph, List<List<Vertex>>>
{
    public override string Name => "Strongly Connected Components";
    public override string Description =>
        "Finds the maximal strongly connected components of a graph using a two-pass depth-first search.";

    public override List<List<Vertex>> Apply(Graph graph)
    {
        return Find(graph);
    }

    public List<List<Vertex>> Find(Graph graph)
    {
        var adjacencyList = graph.ToAdjacencyList();
        var inverseAdjacencyList = BuildInverseAdjacencyList(graph);

        var visited = new HashSet<Vertex>();
        var orderedVertices = new Stack<Vertex>();

        foreach (var vertex in graph.Vertices)
        {
            if (!visited.Contains(vertex))
            {
                FillOrder(vertex, adjacencyList, visited, orderedVertices);
            }
        }

        visited.Clear();
        var components = new List<List<Vertex>>();

        while (orderedVertices.Count > 0)
        {
            var vertex = orderedVertices.Pop();
            if (!visited.Contains(vertex))
            {
                var component = new List<Vertex>();
                CollectComponent(vertex, inverseAdjacencyList, visited, component);
                components.Add(component);
            }
        }

        return components;
    }

    private static void FillOrder(
        Vertex vertex,
        Dictionary<Vertex, List<Vertex>> adjacencyList,
        HashSet<Vertex> visited,
        Stack<Vertex> orderedVertices
    )
    {
        visited.Add(vertex);

        foreach (var neighbor in adjacencyList[vertex])
        {
            if (!visited.Contains(neighbor))
            {
                FillOrder(neighbor, adjacencyList, visited, orderedVertices);
            }
        }

        orderedVertices.Push(vertex);
    }

    private static void CollectComponent(
        Vertex vertex,
        Dictionary<Vertex, List<Vertex>> adjacencyList,
        HashSet<Vertex> visited,
        List<Vertex> component
    )
    {
        visited.Add(vertex);
        component.Add(vertex);

        foreach (var neighbor in adjacencyList[vertex])
        {
            if (!visited.Contains(neighbor))
            {
                CollectComponent(neighbor, adjacencyList, visited, component);
            }
        }
    }

    private static Dictionary<Vertex, List<Vertex>> BuildInverseAdjacencyList(Graph graph)
    {
        var inverseAdjacencyList = new Dictionary<Vertex, List<Vertex>>();

        foreach (var vertex in graph.Vertices)
        {
            inverseAdjacencyList[vertex] = new List<Vertex>();
        }

        foreach (var edge in graph.Edges)
        {
            inverseAdjacencyList[edge.To].Add(edge.From);

            if (graph.IsUndirected)
            {
                inverseAdjacencyList[edge.From].Add(edge.To);
            }
        }

        return inverseAdjacencyList;
    }
}
