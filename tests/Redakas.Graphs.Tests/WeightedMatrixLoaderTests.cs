using Redakas.Graphs.Entities;
using Redakas.Graphs.Helpers;

namespace Redakas.Graphs.Tests;

public class WeightedMatrixLoaderTests
{
    private const string Json = """
        {
          "A": { "A": 0 },
          "B": { "A": 10, "B": 0 },
          "Cé": { "A": 20, "B": 5, "Cé": 0 }
        }
        """;

    [Fact]
    public void Parse_NormalizesAliases()
    {
        var aliases = new Dictionary<string, string> { ["Ce"] = "Cé" };
        const string jsonComAlias = """{ "Ce": { "A": 1 } }""";

        var matriz = WeightedMatrixLoader.Parse(jsonComAlias, aliases);

        Assert.True(matriz.ContainsKey("Cé"));
        Assert.False(matriz.ContainsKey("Ce"));
    }

    [Fact]
    public void BuildGraph_UndirectedWeighted_SkipsSelfAndZero()
    {
        var matriz = WeightedMatrixLoader.Parse(Json, aliases: null);

        var (grafo, vertices) = WeightedMatrixLoader.BuildGraph(matriz);

        Assert.Equal(3, grafo.Vertices.Count);
        Assert.Equal(3, grafo.Edges.Count); // A-B, A-Cé, B-Cé; sem self-loops
        Assert.True(grafo.IsUndirected);
        Assert.True(grafo.IsWeighted);
        Assert.Equal(10, grafo.Edges.Single(e =>
            (e.From == vertices["A"] && e.To == vertices["B"]) ||
            (e.From == vertices["B"] && e.To == vertices["A"])).Weight);
    }

    [Fact]
    public void BuildGraph_IsolatedVertex_KeptWithoutEdges()
    {
        const string json = """
            { "A": { "A": 0 }, "B": { "B": 0 } }
            """;
        var matriz = WeightedMatrixLoader.Parse(json, aliases: null);

        var (grafo, vertices) = WeightedMatrixLoader.BuildGraph(matriz);

        Assert.Equal(2, grafo.Vertices.Count);
        Assert.Empty(grafo.Edges);
        Assert.True(vertices.ContainsKey("B"));
    }

    [Fact]
    public void ComCongestionamento_MultiplicaSomenteTrechos()
    {
        var matriz = WeightedMatrixLoader.Parse(Json, aliases: null);
        var (grafo, v) = WeightedMatrixLoader.BuildGraph(matriz);

        var congestionado = CongestionHelper.ComCongestionamento(
            grafo, [(v["A"], v["B"])], fator: 4);

        double PesoEntre(Graph g, Vertex x, Vertex y) => g.Edges.Single(e =>
            (e.From == x && e.To == y) || (e.From == y && e.To == x)).Weight;

        Assert.Equal(40, PesoEntre(congestionado, v["A"], v["B"]));
        Assert.Equal(5, PesoEntre(congestionado, v["B"], v["Cé"]));
        Assert.Equal(10, PesoEntre(grafo, v["A"], v["B"])); // original intacto
    }

    [Fact]
    public void PesoDoTrecho_BuscaNasDuasDirecoes()
    {
        var matriz = WeightedMatrixLoader.Parse(Json, aliases: null);
        var (grafo, v) = WeightedMatrixLoader.BuildGraph(matriz);

        Assert.Equal(10, CongestionHelper.PesoDoTrecho(grafo, v["A"], v["B"]));
        Assert.Equal(10, CongestionHelper.PesoDoTrecho(grafo, v["B"], v["A"]));
    }
}
