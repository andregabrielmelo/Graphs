namespace Graphs.Main.Entities;

internal class Node
{
    public string Name { get; private set; }
    public string? Value { get; private set; }

    public Node(string name)
    {
        Name = name;
    }

    public Node(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
