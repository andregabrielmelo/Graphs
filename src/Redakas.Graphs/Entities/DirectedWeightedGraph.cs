namespace Redakas.Graphs.Entities;

public class DirectedWeightedGraph(IEnumerable<Vertex> vertexs, IEnumerable<Edge> edges)
    : Graph(vertexs, edges, GraphDirection.Directed, GraphFeatures.Weighted);
