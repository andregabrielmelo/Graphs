using Graphs.Main.Entities.Graphs;
using Graphs.Main.Interfaces;

namespace Graphs.Main.Entities.Algorithms;

internal class BreathFirstSearchAlgorithm : PathFindingAlgorithm<Vertex>
{
    public override string Name => "Breath First Search";
    public override string Description => throw new NotImplementedException();

    public override List<Vertex> Find(Vertex start, Vertex end, ISearchable<Vertex> graph)
    {
        var visited = new Dictionary<Vertex, Vertex>();
        visited.Add(start, null);

        var frontier = new Queue<Vertex>();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            var neighbors = graph.GetNeighbors(current);
            foreach (var tile in neighbors)
            {
                if (!visited.ContainsKey(tile))
                {
                    frontier.Enqueue(tile);
                    visited.Add(tile, current);
                }
            }
        }

        return ReconstructPath(visited, start, end);
    }
}
