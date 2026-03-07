using Graphs.Main.Entities.Graphs;

namespace Graphs.Main.Builders;

internal class GraphBuilder
{
    private List<Vertex> _vertices = new();
    private List<Edge> _edges = new();
    private GraphDirection _graphDirection;
    private GraphFeatures _graphFeatues;

    private GraphBuilder() { }

    public static GraphBuilder Empty() => new();

    public GraphBuilder WithGraphDirection(GraphDirection graphDirection)
    {
        _graphDirection = graphDirection;
        return this;
    }

    public GraphBuilder WithGraphFeatures(GraphFeatures features)
    {
        _graphFeatues |= features;
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
        if (_graphFeatues == GraphFeatures.Weighted)
        {
            return _graphDirection switch
            {
                GraphDirection.Directed => new DirectedWeightedGraph(_vertices, _edges),
                GraphDirection.Undirected => new UndirectedWeightedGraph(_vertices, _edges),
                _ => throw new InvalidOperationException("Graph direction must be specified"),
            };
        }
        else
        {
            return _graphDirection switch
            {
                GraphDirection.Directed => new DirectedGraph(_vertices, _edges),
                GraphDirection.Undirected => new UndirectedGraph(_vertices, _edges),
                _ => throw new InvalidOperationException("Graph direction must be specified"),
            };
        }
    }
}
