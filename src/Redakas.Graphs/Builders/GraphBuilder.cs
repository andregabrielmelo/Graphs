using Redakas.Graphs.Entities;

namespace Redakas.Graphs.Builders;

public class GraphBuilder
{
    private List<Vertex> _vertices = new();
    private List<Edge> _edges = new();
    private GraphDirection _graphDirection;
    private GraphFeatures _graphFeatures;

    private GraphBuilder() { }

    public static GraphBuilder Empty() => new();

    public GraphBuilder WithGraphDirection(GraphDirection graphDirection)
    {
        _graphDirection = graphDirection;
        return this;
    }

    public GraphBuilder WithGraphFeatures(GraphFeatures features)
    {
        _graphFeatures |= features;
        return this;
    }

    public GraphBuilder WithVertex(Vertex vertex)
    {
        _vertices.Add(vertex);
        return this;
    }

    public GraphBuilder WithVertices(IEnumerable<Vertex> vertices)
    {
        _vertices.AddRange(vertices);
        return this;
    }

    public GraphBuilder WithEdge(Edge edge)
    {
        _edges.Add(edge);
        return this;
    }

    public GraphBuilder WithEdges(IEnumerable<Edge> edges)
    {
        _edges.AddRange(edges);
        return this;
    }

    public Graph Build()
    {
        return new Graph(_vertices, _edges, _graphDirection, _graphFeatures);
    }
}
