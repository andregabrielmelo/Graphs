namespace Graphs.Main.Entities;

internal class Relation
{
    public Node From { get; private set; }
    public Node To { get; private set; }

    public Relation(Node from, Node to)
    {
        From = from;
        To = to;
    }
}
