using Graphs.Main.Builders;
using Graphs.Main.Entities.Algorithms;
using Graphs.Main.Entities.Graphs;
using Graphs.Main.Helpers;

// Define vertices
var states = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" };

// Define edges (from, to)
var edges = new List<(string From, string To)>
{
    ("A", "B"),
    ("A", "C"),
    ("B", "D"),
    ("B", "E"),
    ("C", "F"),
    ("C", "G"),
    ("D", "H"),
    ("E", "F"),
    ("F", "I"),
    ("G", "I"),
    ("H", "C"), // Cycle back to C
    ("I", "J"),
    ("J", "K"),
    ("K", "F"), // Cycle back to F
    ("E", "G"), // Multiple paths to G
    ("B", "G"), // Multiple paths to G
    ("H", "I"), // Another path to I
};

// Build the graph
var builder = GraphBuilder.Empty();
builder.WithGraphDirection(GraphDirection.Directed);

var vertexMap = new Dictionary<string, Vertex>();

// Add vertices
foreach (var state in states)
{
    var vertex = new Vertex<string>(state, "metadata");
    builder.WithVertex(vertex);
    vertexMap[state] = vertex;
}

// Add edges
foreach (var (from, to) in edges)
{
    var edge = new Edge(vertexMap[from], vertexMap[to]);
    builder.WithEdge(edge);
}

// Build the graph
Graph graph = builder.Build();

// Print adjacency matrix
Console.WriteLine("Graph adjacency matrix:");
var matrix = graph.ToAdjacencyMatrix();
MatrixHelpers.ShowMatrix<double?>(matrix);

// Print adjacency list
Console.WriteLine("\nGraph adjacency list:");
var adjacencyList = graph.ToAdjacencyList();
foreach (var vertex in graph.Vertices)
{
    var neighbors = adjacencyList[vertex].Select(v => v.Name);
    Console.WriteLine($"{vertex.Name} -> {string.Join(", ", neighbors)}");
}

// Define start and end vertices
Vertex startVertex = graph.Vertices[0];
Vertex endVertex = graph.Vertices[graph.Vertices.Count - 1];

// List of path finding algorithms
var algorithms = new List<PathFindingAlgorithm<Vertex>>
{
    new DepthFirstSearchAlgorithm(),
    new BreathFirstSearchAlgorithm(),
};

// Loop over algorithms and show results
foreach (var algorithm in algorithms)
{
    Console.WriteLine($"\n=== {algorithm.Name} ===");
    List<Vertex> path = algorithm.Find(startVertex, endVertex, graph);

    Console.WriteLine($"Cost from {startVertex.Name} to {endVertex.Name}: {path.Count}");
    string pathString = string.Join(" -> ", path.Select(v => v.Name));
    Console.WriteLine($"Path: {pathString}");
}
