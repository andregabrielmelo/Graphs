using Redakas.Graphs.Entities;

namespace Redakas.Graphs.Helpers;

/// <summary>
/// Simulação de congestionamento: gera cópia do grafo com peso multiplicado
/// nos trechos indicados (pares de vértices, sem direção).
/// </summary>
public static class CongestionHelper
{
    public static Graph ComCongestionamento(
        Graph original,
        IReadOnlyCollection<(Vertex De, Vertex Para)> trechos,
        double fator)
    {
        bool EhTrecho(Edge e) => trechos.Any(t =>
            (e.From == t.De && e.To == t.Para) || (e.From == t.Para && e.To == t.De));

        var novasArestas = original.Edges
            .Select(e => EhTrecho(e) ? e with { Weight = e.Weight * fator } : e);

        return new Graph(original.Vertices, novasArestas, original.Direction, original.Features);
    }

    /// <summary>Extrai os trechos (pares consecutivos) de uma rota.</summary>
    public static List<(Vertex De, Vertex Para)> TrechosDaRota(List<Vertex> rota) =>
        [.. rota.Zip(rota.Skip(1))];

    /// <summary>Custo total da rota somando pesos das arestas percorridas.</summary>
    public static double CustoDaRota(Graph grafo, List<Vertex> rota) =>
        TrechosDaRota(rota).Sum(t => grafo.Edges.Single(e =>
            (e.From == t.De && e.To == t.Para) || (e.From == t.Para && e.To == t.De)).Weight);
}
