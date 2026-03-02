using Graphs.Main.Builders;
using Graphs.Main.Entities;
using Graphs.Main.Enums;

namespace Graphs.Main.Helpers;

internal class GraphHelper
{
    internal static Graph BuildGraph(
        IEnumerable<string> states,
        List<Tuple<string, string>> relations,
        GraphType graphType
    )
    {
        GraphBuilder builder = GraphBuilder.Empty();

        Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();

        builder.WithGraphType(graphType);

        foreach (string state in states)
        {
            Node node = new Node(state);
            builder.WithNode(node);

            nodeMap.Add(state, node);
        }

        foreach (Tuple<string, string> tuple in relations)
        {
            Node from = nodeMap[tuple.Item1];
            Node to = nodeMap[tuple.Item2];
            Relation relation = new Relation(from, to);
            builder.WithRelation(relation);
        }

        Graph graph = builder.Build();
        return graph;
    }
}
