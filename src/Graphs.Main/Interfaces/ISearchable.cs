namespace Graphs.Main.Interfaces;

public interface ISearchable<T>
    where T : notnull
{
    IReadOnlyList<T> Points { get; }
    IReadOnlyList<T> GetNeighbors(T point);
}
