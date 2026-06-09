using Redakas.Graphs.Entities;
using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Algorithms.Entities;

public class AStar : PathFindingAlgorithm<Vertex>
{
    public override string Name => "A*";

    public override string Description =>
        "Algoritmo de busca heurística que utiliza o custo acumulado e uma estimativa do custo restante.";

    public override List<Vertex> Find(Vertex start, Vertex end, IGraph searchable)
    {
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
        fScore[start] = Heuristica(start, end);

        // Enquanto houver nós para analisar
        while (listaAberta.Count > 0)
        {
            // Seleciona o nó com o menor fScore para analisar
            Vertex atual = listaAberta
                .MinBy(v => fScore.GetValueOrDefault(v, double.MaxValue))!;

            // Se chegamos ao destino, reconstruímos o caminho a partir dos pais
            if (atual == end)
                return ReconstructPath(pais, start, atual);

            // Move o nó atual da lista aberta para a lista fechada
            listaAberta.Remove(atual);
            listaFechada.Add(atual);

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
                fScore[vizinho] = tentativeG + Heuristica(vizinho, end);

                // Se o vizinho não está na lista aberta, adicionamos ele para ser analisado posteriormente
                if (!listaAberta.Contains(vizinho))
                    listaAberta.Add(vizinho);
            }
        }

        return [];
    }

    // Manhattan distance como heurística, adequada para grafos em grade
    // TODO: o certo seria permitir o usuário escolher a heurística, ou criar uma interface IHeuristic para isso. Usar o delegate para isso
    private static double Heuristica(Vertex atual, Vertex destino)
    {
        if (atual.Position is null || destino.Position is null)
            return 0;

        return Math.Abs(atual.Position.X - destino.Position.X)
             + Math.Abs(atual.Position.Y - destino.Position.Y);
    }
}