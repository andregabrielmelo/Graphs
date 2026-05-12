using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Builders;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

namespace Redakas.Graphs.Tests;

public class GraphColoringAlgorithmTests
{
    private static Graph BuildUndirectedGraph(
        IEnumerable<string> vertexNames,
        IEnumerable<(string From, string To)> edgePairs
    )
    {
        var builder = GraphBuilder.Empty().WithGraphDirection(GraphDirection.Undirected);
        var vertexMap = new Dictionary<string, Vertex>();

        foreach (var name in vertexNames)
        {
            var vertex = new Vertex(name);
            builder.WithVertex(vertex);
            vertexMap[name] = vertex;
        }

        foreach (var (from, to) in edgePairs)
            builder.WithEdge(new Edge(vertexMap[from], vertexMap[to]));

        return builder.Build();
    }

    [Fact]
    public void ColorGraph_TwoConnectedVertices_AssignsDifferentColors()
    {
        var graph = BuildUndirectedGraph(["A", "B"], [("A", "B")]);
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);

        Assert.Equal(2, steps.Count);
        Assert.NotEqual(steps[0].Color, steps[1].Color);
    }

    [Fact]
    public void ColorGraph_Triangle_RequiresThreeColors()
    {
        var graph = BuildUndirectedGraph(["A", "B", "C"], [("A", "B"), ("B", "C"), ("A", "C")]);
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);

        Assert.Equal(3, steps.Count);
        Assert.Equal(3, steps.Select(s => s.Color).Distinct().Count());
    }

    [Fact]
    public void ColorGraph_StepNumbersAreSequential()
    {
        var graph = BuildUndirectedGraph(["A", "B", "C"], [("A", "B"), ("B", "C")]);
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);

        for (int i = 0; i < steps.Count; i++)
            Assert.Equal(i + 1, steps[i].StepNumber);
    }

    [Fact]
    public void ColorGraph_AdjacentVerticesNeverShareColor()
    {
        var graph = BuildUndirectedGraph(
            ["A", "B", "C", "D", "E", "F"],
            [("A","B"),("A","C"),("A","E"),("B","C"),("B","D"),("C","D"),("C","F"),("D","E"),("D","F"),("E","F")]
        );
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);
        var colorMap = steps.ToDictionary(s => s.Vertex, s => s.Color);

        foreach (var edge in graph.Edges)
            Assert.NotEqual(colorMap[edge.From], colorMap[edge.To]);
    }

    [Fact]
    public void ColorGraph_SingleVertex_ReturnsOneStepWithColor1()
    {
        var graph = BuildUndirectedGraph(["A"], []);
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);

        Assert.Single(steps);
        Assert.Equal(1, steps[0].Color);
        Assert.Equal(1, steps[0].StepNumber);
    }

    [Fact]
    public void ColorGraph_AllVerticesAreColored()
    {
        var graph = BuildUndirectedGraph(["A", "B", "C", "D"], [("A", "B"), ("B", "C"), ("C", "D")]);
        var algorithm = new GraphColoringAlgorithm();

        List<ColoringStep> steps = algorithm.ColorGraph(graph);

        Assert.Equal(graph.Vertices.Count, steps.Count);
        var coloredVertices = steps.Select(s => s.Vertex).ToHashSet();
        foreach (var vertex in graph.Vertices)
            Assert.Contains(vertex, coloredVertices);
    }
}
