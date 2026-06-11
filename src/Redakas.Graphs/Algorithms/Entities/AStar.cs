using Redakas.Graphs.Entities;
using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Algorithms.Entities;

public class AStar : PathFindingAlgorithm<Vertex>
{
    // Heurística injetada: estimativa H(atual, destino). Null = Manhattan via Position.
    private readonly Func<Vertex, Vertex, double> _heuristica;
    private readonly List<AStarStep> _steps = [];

    /// <summary>
    /// Etapas da última execução de <see cref="Find"/>: a cada iteração, o nó fechado
    /// e o snapshot das listas aberta/fechada (formato dos slides do professor).
    /// </summary>
    public IReadOnlyList<AStarStep> Steps => _steps;

    private AStar() { }

    /// <param name="heuristica">
    /// Estimativa H(atual, destino), ex.: distância em linha reta.
    /// Null = Manhattan via Position (comportamento original).
    /// </param>
    public AStar(Func<Vertex, Vertex, double> heuristica) => _heuristica = heuristica;

    public override string Name => "A*";

    public override string Description =>
        "Algoritmo de busca heurística que utiliza o custo acumulado e uma estimativa do custo restante.";

    public override List<Vertex> Find(Vertex start, Vertex end, IGraph searchable)
    {
        _steps.Clear();

        // Armazena os nós a serem analisados
        var listaAberta = new List<Vertex> { start };
        // Armazena os nós já analisados para evitar voltar a um nó já analisado, ou andar em círculo
        var listaFechada = new HashSet<Vertex>();

        // pais: para reconstruir o caminho ao final, armazenamos o nó pai de cada nó visitado
        var pais = new Dictionary<Vertex, Vertex?>();
        // gScore: custo do caminho mais barato encontrado até o momento para chegar a um nó
        var gScore = new Dictionary<Vertex, double>();
        // fScore: custo estimado total do caminho passando por um nó, ou seja, gScore + heurística
        var fScore = new Dictionary<Vertex, double>();

        var adjacency = searchable.ToAdjacencyList();

        // Calcula as informações iniciais para o nó de início
        gScore[start] = 0;
        fScore[start] = _heuristica.Invoke(start, end);
        pais[start] = null;

        // Enquanto houver nós para analisar
        while (listaAberta.Count > 0)
        {
            // Seleciona o nó com o menor fScore para analisar
            Vertex atual = listaAberta
                .MinBy(v => fScore.GetValueOrDefault(v, double.MaxValue))!;

            // Move o nó atual da lista aberta para a lista fechada
            listaAberta.Remove(atual);
            listaFechada.Add(atual);

            // Registra a etapa (nó fechado + snapshot das listas) para acompanhamento
            double g = gScore.GetValueOrDefault(atual, double.MaxValue);
            double h = _heuristica.Invoke(atual, end);
            _steps.Add(
                new AStarStep(
                    atual,
                    g,
                    h,
                    g + h,
                    [
                        .. listaAberta.Select(v =>
                            new AStarNodeInfo(
                                v,
                                gScore.GetValueOrDefault(v, double.MaxValue),
                                _heuristica.Invoke(v, end),
                                fScore.GetValueOrDefault(v, double.MaxValue)
                            ))
                    ],
                    [.. listaFechada]
                )
            );

            // Se chegamos ao destino, reconstruímos o caminho a partir dos pais
            if (atual == end)
                return ReconstructPath(pais, start, atual);

            // Para cada vizinho do nó atual, calculamos o custo do caminho passando por ele
            if (!adjacency.TryGetValue(atual, out var vizinhos))
                continue;

            // vizinhos é uma lista de tuplas (vizinho, peso), onde peso é o custo de ir do nó atual para o vizinho
            // Para cada vizinho, verificamos se o caminho passando por ele é melhor do que o melhor caminho encontrado até agora
            foreach (var (vizinho, peso) in vizinhos)
            {
                if (listaFechada.Contains(vizinho))
                    continue;

                // Calcula o custo do caminho passando pelo vizinho
                double tentativeG =
                    gScore.GetValueOrDefault(atual, double.MaxValue) + peso;

                double gVizinho =
                    gScore.GetValueOrDefault(vizinho, double.MaxValue);

                // Se o caminho passando pelo vizinho não é melhor do que o
                // melhor caminho encontrado até agora, ignoramos esse vizinho
                if (tentativeG >= gVizinho)
                    continue;

                // Se o caminho passando pelo vizinho é melhor, atualizamos as informações para esse vizinho
                pais[vizinho] = atual;
                gScore[vizinho] = tentativeG;
                fScore[vizinho] = tentativeG + _heuristica.Invoke(vizinho, end);

                // Se o vizinho não está na lista aberta, adicionamos ele para ser analisado posteriormente
                if (!listaAberta.Contains(vizinho))
                    listaAberta.Add(vizinho);
            }
        }

        return [];
    }

}
