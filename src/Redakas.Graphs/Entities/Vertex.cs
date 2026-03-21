namespace Redakas.Graphs.Entities;

public record Vertex(string name, object? Value = null)
{
    public string Name { get; set; } = name;
    public object? Value { get; set; } = Value;

    public Type ValueType => Value?.GetType() ?? typeof(object);

    public override string ToString() => Name;
}

public sealed record Vertex<T>(string name, T typedValue) : Vertex(name, typedValue)
{
    public T ValueTyped => typedValue;
}
