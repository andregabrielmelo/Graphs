using Redakas.Graphs.Algorithms.Exceptions;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Algorithms.Entities;

public class BreadthFirstSearchAlgorithm : PathFindingAlgorithm<Vertex>
{
    public override string Name => "Breadth First Search";
    public override string Description => throw new NotImplementedException();

    public override List<Vertex> Find(Vertex start, Vertex end, IGraph graph)
    {
        var visited = new Dictionary<Vertex, Vertex?>();
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

        if (!visited.ContainsKey(end))
        {
            throw new PathNotFoundException($"No path found between {start.Name} and {end.Name}");
        }

        return ReconstructPath(visited, start, end);
    }
}
