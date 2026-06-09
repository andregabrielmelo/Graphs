namespace Redakas.Graphs.Entities;

public record Vertex(string name, Position? position = null, object? Value = null)
{
    public string Name { get; set; } = name;
    public Position? Position { get; set; } = position;
    public object? Value { get; set; } = Value;

    public Type ValueType => Value?.GetType() ?? typeof(object);

    public override string ToString() => Name;
}

public sealed record Vertex<T>(string name, Position? position = null, T typedValue = default) : Vertex(name, position, typedValue)
{
    public T ValueTyped => typedValue;
}
