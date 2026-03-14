namespace Redakas.Graphs.Entities;

// TODO: implement graph mutability (e.g., adding/removing vertices and edges) and ensure that the graph's internal state remains consistent after such operations
public class DynamicGraph(
    IEnumerable<Vertex> Vertices,
    IEnumerable<Edge> Edges,
    GraphDirection GraphDirection,
    GraphFeatures GraphFeatures
) : Graph(Vertices, Edges, GraphDirection, GraphFeatures)
{
    public void AddVertex(Vertex vertex)
    {
        if (Vertices.Contains(vertex))
        {
            throw new InvalidOperationException(
                $"Vertex {vertex.Name} already exists in the graph."
            );
        }
        Vertices.Add(vertex);
    }

    public void RemoveVertex(Vertex vertex)
    {
        if (!Vertices.Contains(vertex))
        {
            throw new InvalidOperationException(
                $"Vertex {vertex.Name} does not exist in the graph."
            );
        }
        Vertices.Remove(vertex);
        // Remove all edges connected to this vertex
        Edges.RemoveAll(e => e.From.Equals(vertex) || e.To.Equals(vertex));
    }

    public void AddEdge(Edge edge)
    {
        if (Edges.Contains(edge))
        {
            throw new InvalidOperationException(
                $"Edge from {edge.From.Name} to {edge.To.Name} already exists in the graph."
            );
        }
        if (!Vertices.Contains(edge.From) || !Vertices.Contains(edge.To))
        {
            throw new InvalidOperationException(
                $"Both vertices of the edge must exist in the graph. Missing vertex: {(Vertices.Contains(edge.From) ? edge.To.Name : edge.From.Name)}"
            );
        }
        Edges.Add(edge);
    }

    public void RemoveEdge(Edge edge)
    {
        if (!Edges.Contains(edge))
        {
            throw new InvalidOperationException(
                $"Edge from {edge.From.Name} to {edge.To.Name} does not exist in the graph."
            );
        }
        Edges.Remove(edge);
    }
}
