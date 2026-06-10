using System.Text.Json;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

namespace Redakas.Graphs.Helpers;

/// <summary>
/// Carrega matrizes de distância no formato {nome: {nome: peso}} (ex.: distancias.json)
/// e monta grafos não-dirigidos ponderados a partir delas.
/// </summary>
public static class WeightedMatrixLoader
{
    /// <summary>
    /// Desserializa a matriz e normaliza nomes via dicionário de aliases
    /// (chaves externas e internas). Ex.: "São Luís" → "São Luis".
    /// </summary>
    public static Dictionary<string, Dictionary<string, double>> Parse(
        string json,
        IReadOnlyDictionary<string, string>? aliases)
    {
        var bruto = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json)
            ?? throw new InvalidOperationException("JSON de distâncias vazio ou inválido.");

        string Norm(string nome) =>
            aliases is not null && aliases.TryGetValue(nome, out var canonico) ? canonico : nome;

        var matriz = new Dictionary<string, Dictionary<string, double>>();
        foreach (var (de, linha) in bruto)
        {
            var linhaNorm = matriz.TryGetValue(Norm(de), out var existente)
                ? existente
                : matriz[Norm(de)] = [];
            foreach (var (para, peso) in linha)
                linhaNorm[Norm(para)] = peso;
        }

        return matriz;
    }

    /// <summary>
    /// Monta grafo não-dirigido ponderado: um vértice por nome (mesmo sem arestas),
    /// uma aresta por par com peso > 0 (self-loops e duplicatas ignorados).
    /// </summary>
    public static (Graph Grafo, Dictionary<string, Vertex> Vertices) BuildGraph(
        Dictionary<string, Dictionary<string, double>> matriz)
    {
        var nomes = matriz.Keys
            .Union(matriz.Values.SelectMany(l => l.Keys))
            .OrderBy(n => n)
            .ToList();

        var vertices = nomes.ToDictionary(n => n, n => new Vertex(n));
        var arestas = new List<Edge>();
        var paresVistos = new HashSet<(string, string)>();

        foreach (var (de, linha) in matriz)
        {
            foreach (var (para, peso) in linha)
            {
                if (de == para || peso <= 0)
                    continue;

                var par = string.CompareOrdinal(de, para) < 0 ? (de, para) : (para, de);
                if (!paresVistos.Add(par))
                    continue;

                arestas.Add(new Edge(vertices[de], vertices[para], peso));
            }
        }

        var grafo = new Graph(vertices.Values, arestas,
            GraphDirection.Undirected, GraphFeatures.Weighted);
        return (grafo, vertices);
    }

    /// <summary>
    /// Cria heurística de lookup: H(atual, destino) = valor da matriz (ex.: linha reta IBGE).
    /// Par ausente → 0 (heurística admissível neutra).
    /// </summary>
    public static Func<Vertex, Vertex, double> BuildHeuristica(
        Dictionary<string, Dictionary<string, double>> matriz)
    {
        return (atual, destino) =>
        {
            if (matriz.TryGetValue(atual.Name, out var linha) &&
                linha.TryGetValue(destino.Name, out var d))
                return d;
            if (matriz.TryGetValue(destino.Name, out var linhaInv) &&
                linhaInv.TryGetValue(atual.Name, out var dInv))
                return dInv;
            return 0;
        };
    }
}
