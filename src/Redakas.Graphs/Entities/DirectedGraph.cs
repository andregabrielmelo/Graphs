namespace Redakas.Graphs.Entities;

public class DirectedGraph(IEnumerable<Vertex> vertexs, IEnumerable<Edge> edges)
    : Graph(vertexs, edges, GraphDirection.Directed, GraphFeatures.None);
