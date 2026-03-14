namespace Redakas.Graphs.Entities;

public class UndirectedWeightedGraph(IEnumerable<Vertex> nodes, IEnumerable<Edge> edges)
    : Graph(nodes, edges, GraphDirection.Undirected, GraphFeatures.Weighted);
