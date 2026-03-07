namespace Graphs.Main.Entities.Graphs;

public abstract record Vertex(string Name)
{
    public abstract string Name { get; }
    public abstract object? Value { get; }
    public abstract Type ValueType { get; }
}

public record Vertex<T>(string Name, T TypedValue) : Vertex(Name)
{
    public override string Name { get; } = Name;
    public override object? Value => TypedValue;
    public override Type ValueType => typeof(T);

    public T ValueTyped => TypedValue;
}
