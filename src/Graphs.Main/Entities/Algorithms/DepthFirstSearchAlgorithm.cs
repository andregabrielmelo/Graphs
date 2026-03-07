using Graphs.Main.Entities.Graphs;
using Graphs.Main.Interfaces;

namespace Graphs.Main.Entities.Algorithms;

internal class DepthFirstSearchAlgorithm : PathFindingAlgorithm<Vertex>
{
    public override string Name => "Depth First Search";
    public override string Description =>
        "Depth First Search (DFS) is a graph traversal algorithm that explores as far as possible along each branch before backtracking. It uses a stack data structure, either explicitly or through recursion, to keep track of the vertices to be explored. DFS can be used for various applications, such as finding connected components, topological sorting, and solving puzzles.";

    public override List<Vertex> Find(Vertex start, Vertex end, ISearchable<Vertex> graph)
    {
        var visited = new Dictionary<Vertex, Vertex?>();
        visited.Add(start, null);

        var frontier = new Stack<Vertex>();
        frontier.Push(start);

        while (frontier.Count > 0)
        {
            Vertex current = frontier.Pop();
            if (current == end)
            {
                break;
            }

            var neighbors = graph.GetNeighbors(current);
            foreach (var vertex in neighbors)
            {
                if (!visited.ContainsKey(vertex))
                {
                    frontier.Push(vertex);
                    visited.Add(vertex, current);
                }
            }
        }

        return ReconstructPath(visited, start, end);
    }
}
