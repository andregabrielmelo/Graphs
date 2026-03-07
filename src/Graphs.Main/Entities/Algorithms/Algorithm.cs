namespace Graphs.Main.Entities.Algorithms;

internal abstract class Algorithm<T, TResult>
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract TResult Apply(T value);
}
