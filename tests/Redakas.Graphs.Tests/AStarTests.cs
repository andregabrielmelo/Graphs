using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

namespace Redakas.Graphs.Tests;

public class AStarTests
{
    // Grafo linha: A -1- B -1- C, atalho caro A -5- C
    private static (Graph grafo, Vertex a, Vertex b, Vertex c) BuildLineGraph()
    {
        var a = new Vertex("A", new Position(0, 0));
        var b = new Vertex("B", new Position(1, 0));
        var c = new Vertex("C", new Position(2, 0));
        var grafo = new Graph(
            [a, b, c],
            [new Edge(a, b, 1), new Edge(b, c, 1), new Edge(a, c, 5)],
            GraphDirection.Undirected,
            GraphFeatures.Weighted);
        return (grafo, a, b, c);
    }

    [Fact]
    public void Find_UsesInjectedHeuristic()
    {
        var (grafo, a, _, c) = BuildLineGraph();
        var chamadas = new List<(Vertex, Vertex)>();
        var astar = new AStar((atual, destino) =>
        {
            chamadas.Add((atual, destino));
            return 0;
        });

        var rota = astar.Find(a, c, grafo);

        Assert.Equal(["A", "B", "C"], rota.Select(v => v.Name));
        Assert.NotEmpty(chamadas);
        Assert.All(chamadas, ch => Assert.Equal(c, ch.Item2));
    }

    [Fact]
    public void Find_DefaultHeuristic_StillManhattan()
    {
        var (grafo, a, _, c) = BuildLineGraph();
        var astar = new AStar();

        var rota = astar.Find(a, c, grafo);

        Assert.Equal(["A", "B", "C"], rota.Select(v => v.Name));
    }

    [Fact]
    public void Find_PopulatesSteps_ClosedGrowsByOne_NoOverlap()
    {
        var (grafo, a, _, c) = BuildLineGraph();
        var astar = new AStar((_, _) => 0);

        astar.Find(a, c, grafo);

        Assert.NotEmpty(astar.Steps);
        for (int i = 0; i < astar.Steps.Count; i++)
        {
            var step = astar.Steps[i];
            Assert.Equal(i + 1, step.Fechados.Count);
            Assert.Empty(step.Abertos.Intersect(step.Fechados));
            Assert.Equal(step.G + step.H, step.F);
        }
        Assert.Equal(c, astar.Steps[^1].Fechado);
        Assert.Equal(a, astar.Steps[0].Fechado);
    }

    [Fact]
    public void Find_RunTwice_StepsReset()
    {
        var (grafo, a, b, c) = BuildLineGraph();
        var astar = new AStar((_, _) => 0);

        astar.Find(a, c, grafo);
        int primeira = astar.Steps.Count;
        astar.Find(a, b, grafo);

        Assert.True(astar.Steps.Count <= primeira);
        Assert.Equal(b, astar.Steps[^1].Fechado);
    }
}
