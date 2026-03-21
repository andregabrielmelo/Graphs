using Redakas.Graphs.Entities;

namespace Redakas.Graphs.Interfaces;

public interface IDynamicGraph
{
    public void AddVertex(Vertex vertex);
    public void RemoveVertex(Vertex vertex);
    public void AddEdge(Edge edge);
    public void RemoveEdge(Edge edge);
}
