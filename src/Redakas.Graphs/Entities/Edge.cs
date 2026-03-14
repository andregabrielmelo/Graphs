namespace Redakas.Graphs.Entities;

public record Edge(Vertex From, Vertex To, double Weight = 1)
{
    public override string ToString()
    {
        return $"Edge(From: {From}, To: {To}, Weight: {Weight})";
    }
}
