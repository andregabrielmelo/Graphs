using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Algorithms.Entities;

public abstract class PathFindingAlgorithm<T>
    : Algorithm<(T start, T end, IGraph<T> searchable), IEnumerable<T>>
    where T : notnull
{
    public override List<T> Apply((T start, T end, IGraph<T> searchable) value)
    {
        return Find(value.start, value.end, value.searchable);
    }

    public abstract List<T> Find(T start, T end, IGraph<T> searchable);

    public static List<T> ReconstructPath(Dictionary<T, T?> visited, T start, T end)
    {
        var path = new List<T>();
        var current = end;

        while (current != null)
        {
            path.Add(current);
            current = visited[current];
        }

        path.Reverse();
        return path.Count > 0 && path[0].Equals(start) ? path : [];
    }
}
