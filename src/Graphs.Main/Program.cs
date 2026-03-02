// See https://aka.ms/new-console-template for more information
using Graphs.Main.Builders;
using Graphs.Main.Entities;
using Graphs.Main.Enums;

List<string> states = new List<string>(["1", "2", "3", "4"]);
List<Tuple<string, string>> relations = new List<Tuple<string, string>>
    ([
        Tuple.Create("1", "2"),
        Tuple.Create("1", "3"),
        Tuple.Create("2", "3"),
        Tuple.Create("2", "4"),
        Tuple.Create("3", "4")
    ]);

GraphBuilder builder = GraphBuilder.Empty();

Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();

builder.WithGraphType(GraphType.Directed);

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

Console.WriteLine("Graph adjacency matrix: ");

int[,] adjacencyMatrix = graph.ToAdjacencyMatrix();

int rowLength = adjacencyMatrix.GetLength(0);
int colLength = adjacencyMatrix.GetLength(1);

for (int i = 0; i < rowLength; i++)
{
    for (int j = 0; j < colLength; j++)
    {
        Console.Write(string.Format("{0} ", adjacencyMatrix[i, j]));
    }

    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine("Graph adjacency list: ");

Dictionary<Node, List<Node>> adjacencyList = graph.ToAdjacencyList();

foreach (Node node in graph.Nodes)
{
    List<string> adjancyListNames = adjacencyList[node].Select(node => node.Name).ToList();
    Console.WriteLine($"{node.Name} -> {String.Join(",", adjancyListNames)}");
}
