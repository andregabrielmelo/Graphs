using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Helpers;

namespace Redakas.Graphs.Tests;

public class AStarCapitaisTests
{
    // Aliases: nomes da tabela IBGE (linha reta) → nomes do distancias.json
    public static readonly Dictionary<string, string> Aliases = new()
    {
        ["São Luís"] = "São Luis",
        ["Aracaju"] = "Aracajú",
        ["Acaraju"] = "Aracajú",
    };

    private static Dictionary<string, Dictionary<string, double>> Estradas() =>
        WeightedMatrixLoader.Parse(File.ReadAllText("data/distancias.json"), Aliases);

    private static Dictionary<string, Dictionary<string, double>> LinhaReta() =>
        WeightedMatrixLoader.Parse(
            File.ReadAllText("data/distancias_em_linha_reta.json"), Aliases);

    [Fact]
    public void NomesDosDoisArquivosCasamAposAliases()
    {
        var nomesEstrada = Estradas().Keys.ToHashSet();
        var nomesLinhaReta = LinhaReta().Keys.ToHashSet();

        Assert.Equal(27, nomesEstrada.Count);
        // Todo nome da linha reta existe no grafo de estradas
        Assert.Empty(nomesLinhaReta.Except(nomesEstrada));
    }

    [Fact]
    public void GrafoTem27CapitaisEMacapaIsolado()
    {
        var (grafo, vertices) = WeightedMatrixLoader.BuildGraph(Estradas());

        Assert.Equal(27, grafo.Vertices.Count);
        var macapa = vertices["Macapá"];
        Assert.DoesNotContain(grafo.Edges, e => e.From == macapa || e.To == macapa);
    }

    [Fact]
    public void HeuristicaAdmissivel_LinhaRetaNaoExcedeEstrada()
    {
        var estradas = Estradas();
        var linhaReta = LinhaReta();

        foreach (var (de, linha) in estradas)
        {
            foreach (var (para, estrada) in linha)
            {
                if (de == para || estrada <= 0) continue;
                if (!linhaReta.TryGetValue(de, out var lr) ||
                    !lr.TryGetValue(para, out var reta))
                {
                    if (!linhaReta.TryGetValue(para, out var lrInv) ||
                        !lrInv.TryGetValue(de, out reta))
                        continue;
                }
                Assert.True(reta <= estrada,
                    $"{de}→{para}: linha reta {reta} > estrada {estrada}");
            }
        }
    }

    [Fact]
    public void CongestionamentoForcaRotaDiferente()
    {
        var (grafo, v) = WeightedMatrixLoader.BuildGraph(Estradas());
        var heuristica = WeightedMatrixLoader.BuildHeuristica(LinhaReta());
        var astar = new AStar(heuristica);

        var rotaNormal = astar.Find(v["São Paulo"], v["Fortaleza"], grafo);
        Assert.NotEmpty(rotaNormal);

        var congestionado = CongestionHelper.ComCongestionamento(
            grafo, CongestionHelper.TrechosDaRota(rotaNormal), fator: 4);
        var rotaCongestionada = astar.Find(v["São Paulo"], v["Fortaleza"], congestionado);

        Assert.NotEmpty(rotaCongestionada);
        Assert.NotEqual(
            rotaNormal.Select(x => x.Name).ToList(),
            rotaCongestionada.Select(x => x.Name).ToList());
        Assert.NotEmpty(astar.Steps);
    }

    [Fact]
    public void MacapaComoDestino_RetornaVazio()
    {
        var (grafo, v) = WeightedMatrixLoader.BuildGraph(Estradas());
        var astar = new AStar(WeightedMatrixLoader.BuildHeuristica(LinhaReta()));

        var rota = astar.Find(v["São Paulo"], v["Macapá"], grafo);

        Assert.Empty(rota);
    }
}
