using Redakas.Graphs.Entities;

namespace Redakas.Graphs.Interfaces;

public interface IGraph
{
    public Dictionary<Vertex, List<(Vertex To, double Weight)>> ToAdjacencyList();
    public double[,] ToAdjacencyMatrix(double sameVertex = 1.0, double defaultValue = 0.0);
    public double[,] ToReachabilityMatrix();
    public double[,] ToDistanceMatrix();
    public List<Vertex> VertexTransitiveClosure(Vertex vertex);
    public List<Vertex> VertexDirectTransitiveClosure(Vertex vertex);
    public List<Vertex> VertexInverseTransitiveClosure(Vertex vertex);
    public List<List<Vertex>> GetStronglyConnectedComponents();
    public bool HasVertex(Vertex vertex);
    public bool HasEdge(Vertex from, Vertex to);
    public bool HasEdge(Edge edge);
    public IEnumerable<Vertex> GetNeighbors(Vertex vertex);
}
