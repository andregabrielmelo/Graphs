namespace Redakas.Graphs.Entities;

public abstract record Matrix()
{
    public abstract object? Value { get; }
    public abstract Type ValueType { get; }
}

public record Matrix<T>(T TypedValue) : Matrix()
{
    public override object? Value => TypedValue;
    public override Type ValueType => typeof(T);

    public T ValueTyped => TypedValue;
}
