using Graphs.Main.Interfaces;

namespace Graphs.Main.Entities.Algorithms;

internal abstract class PathFindingAlgorithm<T>
    : Algorithm<(T start, T end, ISearchable<T> searchable), List<T>>
    where T : notnull
{
    public override List<T> Apply((T start, T end, ISearchable<T> searchable) value)
    {
        return Find(value.start, value.end, value.searchable);
    }

    public abstract List<T> Find(T start, T end, ISearchable<T> searchable);

    public static List<T> ReconstructPath(Dictionary<T, T> visited, T start, T end)
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
