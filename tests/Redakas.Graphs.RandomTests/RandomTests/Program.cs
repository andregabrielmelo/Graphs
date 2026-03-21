using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Algorithms.Exceptions;
using Redakas.Graphs.Builders;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;
using Redakas.Graphs.Helpers;

// Necessary to properly display the infinity symbol in the console output
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Define vertices
var states = new List<string> { "A", "B", "C", "D", "E" };

// Define edges (from, to)
var edges = new List<(string From, string To)>
{
    ("A", "B"),
    ("B", "C"),
    ("C", "D"),
    ("D", "E"),
    ("E", "C"),
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
var adjacencyMatrix = graph.ToAdjacencyMatrix();
MatrixHelpers.ShowMatrix<double>(adjacencyMatrix);

// Print reachbility matrix (transitive closure)
Console.WriteLine("\nGraph reachbility matrix:");
var reachbilityMatrix = graph.ToReachabilityMatrix();
MatrixHelpers.ShowMatrix<double>(reachbilityMatrix);

// Print distance matrix
Console.WriteLine("\nGraph distance matrix:");
var distanceMatrix = graph.ToDistanceMatrix();
MatrixHelpers.ShowMatrix<double>(distanceMatrix);

// Print adjacency list
Console.WriteLine("\nGraph adjacency list:");
var adjacencyList = graph.ToAdjacencyList();
foreach (var vertex in graph.Vertices)
{
    var neighbors = adjacencyList[vertex].Select(edge => edge.To.Name);
    Console.WriteLine($"{vertex.Name} -> {string.Join(", ", neighbors)}");
}

// Print transitive and instrasitive closure of a vertex in the graph
int vertexIndex = 0; // Index of the vertex to analyze (e.g., 0 for "A")
Vertex graphVertex = graph.Vertices[vertexIndex];
string vertexName = graphVertex.Name;
List<Vertex> vertexDirectTransitiveClouse = graph.VertexDirectTransitiveClosure(graphVertex);
List<Vertex> vertexIndirecttransitiveClouse = graph.VertexInverseTransitiveClosure(graphVertex);
Console.WriteLine(
    $"\nVertex {vertexName} direct transitive closure: {String.Join(",", vertexDirectTransitiveClouse)}"
);
Console.WriteLine(
    $"\nVertex {vertexName} indirect transitive closure: {String.Join(",", vertexIndirecttransitiveClouse)}"
);

// Define start and end vertices
Vertex startVertex = graph.Vertices[0];
Vertex endVertex = graph.Vertices[graph.Vertices.Count - 1];

// List of path finding algorithms
var algorithms = new List<PathFindingAlgorithm<Vertex>>
{
    new DepthFirstSearchAlgorithm(),
    new BreadthFirstSearchAlgorithm(),
    // TODO:
    //new DijkstraSearchAlgorithm(),
    //new GreedyBestFirstSearchAlgorithm(),
    //new MinimaxSearchAlgorithm(),
    //new AlphaBetaPruningSearchAlgorithm(),
    //new AStarSearchAlgorithm(),
    //new MaximumFluSearchAlgorithm()
};

// Loop over algorithms and show results
foreach (var algorithm in algorithms)
{
    Console.WriteLine($"\n=== {algorithm.Name} ===");
    try
    {
        List<Vertex> path = algorithm.Find(startVertex, endVertex, graph);
        Console.WriteLine($"Cost from {startVertex.Name} to {endVertex.Name}: {path.Count}");
        string pathString = string.Join(" -> ", path.Select(v => v.Name));
        Console.WriteLine($"Path: {pathString}");
    }
    catch (PathNotFoundException ex)
    {
        Console.WriteLine("\n" + ex.Message);
    }
}
