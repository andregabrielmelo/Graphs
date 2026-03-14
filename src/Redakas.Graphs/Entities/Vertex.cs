namespace Redakas.Graphs.Entities;

public abstract record Vertex(string Name)
{
    public abstract object? Value { get; }
    public Type ValueType => Value?.GetType() ?? typeof(object);

    public override string ToString() => Name;
}

public sealed record Vertex<T>(string Name, T TypedValue) : Vertex(Name)
{
    public T ValueTyped => TypedValue;
    public override object? Value => TypedValue;
}
