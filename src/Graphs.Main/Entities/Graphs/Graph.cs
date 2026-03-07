using Graphs.Main.Interfaces;

namespace Graphs.Main.Entities.Graphs;

public abstract class Graph(
    IEnumerable<Vertex> Vertices,
    IEnumerable<Edge> Edges,
    GraphDirection GraphDirection,
    GraphFeatures GraphFeatures
) : ISearchable<Vertex>
{
    public List<Vertex> Vertices { get; init; } = Vertices.ToList();
    public List<Edge> Edges { get; init; } = Edges.ToList();
    public GraphDirection Direction { get; init; } = GraphDirection;
    public GraphFeatures Features { get; init; } = GraphFeatures;

    public bool IsWeighted => Features.HasFlag(GraphFeatures.Weighted);
    public bool IsDirected => Direction == GraphDirection.Directed;
    public bool IsUndirected => Direction == GraphDirection.Undirected;

    public abstract Dictionary<Vertex, List<Vertex>> ToAdjacencyList();

    public abstract double?[,] ToAdjacencyMatrix();
    IReadOnlyList<Vertex> ISearchable<Vertex>.Points => Vertices;

    public IReadOnlyList<Vertex> GetNeighbors(Vertex point)
    {
        List<Vertex> neighbours = ToAdjacencyList()[point];
        return neighbours;
    }
}
