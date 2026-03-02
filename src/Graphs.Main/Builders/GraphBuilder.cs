using Graphs.Main.Entities;
using Graphs.Main.Enums;

namespace Graphs.Main.Builders;

internal class GraphBuilder
{
    private GraphType _graphType;
    private List<Node> _nodes = new List<Node>();
    private List<Relation> _relations = new List<Relation>();

    private GraphBuilder() { }

    public static GraphBuilder Empty() => new GraphBuilder();

    public GraphBuilder WithGraphType(GraphType graphType)
    {
        _graphType = graphType;
        return this;
    }

    public GraphBuilder WithNode(Node node)
    {
        _nodes.Add(node);
        return this;
    }

    public GraphBuilder WithNodes(IEnumerable<Node> nodes)
    {
        _nodes.AddRange(nodes);
        return this;
    }

    public GraphBuilder WithRelation(Relation relation)
    {
        _relations.Add(relation);
        return this;
    }

    public GraphBuilder WithRelations(IEnumerable<Relation> relations)
    {
        _relations.AddRange(relations);
        return this;
    }

    public Graph Build()
    {
        return _graphType switch
        {
            GraphType.Undirected => new UndirectedGraph(_nodes, _relations),
            GraphType.Directed => new DirectedGraph(_nodes, _relations),
            _ => throw new ArgumentOutOfRangeException(
                nameof(_graphType),
                $"Not expected graph type value: {_graphType}"
            ),
        };
    }
}
