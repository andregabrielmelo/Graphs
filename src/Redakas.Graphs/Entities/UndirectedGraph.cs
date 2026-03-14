namespace Redakas.Graphs.Entities;

public class UndirectedGraph(IEnumerable<Vertex> nodes, IEnumerable<Edge> edges)
    : Graph(nodes, edges, GraphDirection.Undirected, GraphFeatures.None);
