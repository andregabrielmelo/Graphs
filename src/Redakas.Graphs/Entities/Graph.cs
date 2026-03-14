using System.Data;
using Redakas.Graphs.Helpers;
using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Entities;

public abstract class Graph(
    IEnumerable<Vertex> Vertices,
    IEnumerable<Edge> Edges,
    GraphDirection GraphDirection,
    GraphFeatures GraphFeatures
) : IGraph<Vertex>
{
    public List<Vertex> Vertices { get; init; } = [.. Vertices];
    public List<Edge> Edges { get; init; } = [.. Edges];
    public GraphDirection Direction { get; init; } = GraphDirection;
    public GraphFeatures Features { get; init; } = GraphFeatures;

    public bool IsWeighted => Features.HasFlag(GraphFeatures.Weighted);
    public bool IsDirected => Direction == GraphDirection.Directed;
    public bool IsUndirected => Direction == GraphDirection.Undirected;
    public bool IsConnected => ToReachabilityMatrix().Cast<double>().All(value => value == 1.0);

    public override string ToString()
    {
        string verticesStr = string.Join(", ", Vertices.Select(v => v.ToString()));
        string edgesStr = string.Join(", ", Edges.Select(e => e.ToString()));
        return $"Graph(Vertices: [{verticesStr}], Edges: [{edgesStr}], Direction: {Direction}, Features: {Features})";
    }

    /// <summary>
    /// Creates an adjacency list representation of the graph, mapping each vertex to a list of its connected vertices
    /// and the associated edge weights.
    /// </summary>
    /// <remarks>The returned adjacency list includes all vertices, even those without outgoing edges. The
    /// list of adjacent vertices for such vertices will be empty.</remarks>
    /// <returns>A dictionary where each key is a vertex and the value is a list of tuples containing the adjacent vertex and the
    /// weight of the connecting edge. If the graph is undirected, each edge is represented in both directions.</returns>
    public Dictionary<Vertex, List<(Vertex To, double Weight)>> ToAdjacencyList()
    {
        Dictionary<Vertex, List<(Vertex To, double Weight)>> list = new();

        foreach (var vertex in Vertices)
        {
            list[vertex] = [];
        }

        foreach (var edge in Edges)
        {
            Vertex from = edge.From;
            Vertex to = edge.To;

            list[from].Add((to, edge.Weight));
            if (IsUndirected)
            {
                list[to].Add((from, edge.Weight));
            }
        }

        return list;
    }

    /// <summary>
    /// Creates an adjacency matrix representation of the graph, assigning weights to edges and default values to
    /// non-edges.
    /// </summary>
    /// <remarks>If the graph is undirected, the matrix is symmetric. The order of vertices in the matrix
    /// corresponds to their order in the Vertices collection.</remarks>
    /// <param name="sameVertex">The value to assign to the diagonal elements of the matrix, representing connections from a vertex to itself.</param>
    /// <param name="defaultValue">The value to assign to matrix elements where no edge exists between two distinct vertices.</param>
    /// <returns>A two-dimensional array of doubles representing the adjacency matrix of the graph. Each element contains the
    /// weight of the edge between two vertices, or the specified default value if no edge exists.</returns>
    public double[,] ToAdjacencyMatrix(double sameVertex = 1.0, double defaultValue = 0.0)
    {
        int size = Vertices.Count;
        double[,] matrix = new double[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                matrix[i, j] = i == j ? sameVertex : defaultValue;
            }
        }

        var indexMap = Vertices.Select((v, i) => new { v, i }).ToDictionary(x => x.v, x => x.i);

        foreach (var edge in Edges)
        {
            int fromIndex = indexMap[edge.From];
            int toIndex = indexMap[edge.To];

            matrix[fromIndex, toIndex] = edge.Weight;
            if (IsUndirected)
            {
                matrix[toIndex, fromIndex] = edge.Weight;
            }
        }

        return matrix;
    }

    /// <summary>
    /// Computes the reachability matrix for the graph using Warshall's algorithm.
    /// </summary>
    /// <remarks>The reachability matrix shows which vertices are reachable from others, regardless of the
    /// number of intermediate steps. Each element is set to 1.0 if a path exists between the vertices; otherwise, it is
    /// 0.0. The returned matrix uses the same vertex ordering as the adjacency matrix produced by the graph.</remarks>
    /// <returns>A two-dimensional array representing the reachability matrix, where each element indicates whether a path exists
    /// between the corresponding pair of vertices.</returns>
    public double[,] ToReachabilityMatrix()
    {
        var numberOfVertices = Vertices.Count;
        var adjacencyMatrix = ToAdjacencyMatrix();

        double[,] reachabilityMatrix = adjacencyMatrix;
        for (int k = 0; k < numberOfVertices; k++) // Warshall's algorithm iterates through each vertex as an intermediate point
        {
            for (int i = 0; i < numberOfVertices; i++)
            {
                for (int j = 0; j < numberOfVertices; j++)
                {
                    reachabilityMatrix[i, j] = Math.Max(
                        reachabilityMatrix[i, j],
                        (
                            (reachabilityMatrix[i, k] == 1.0 && reachabilityMatrix[k, j] == 1.0)
                                ? 1.0
                                : 0.0
                        )
                    );
                }
            }
        }

        return reachabilityMatrix;
    }

    /// <summary>
    /// Calculates the shortest path distances between all pairs of vertices in the graph.
    /// </summary>
    /// <remarks>The returned matrix uses the order of vertices as defined in the graph's vertex collection.
    /// This method is suitable for dense graphs and may be computationally intensive for large graphs.</remarks>
    /// <returns>A two-dimensional array of doubles representing the shortest distances between each pair of vertices. Each
    /// element at position [i, j] contains the shortest distance from vertex i to vertex j. If no path exists, the
    /// value is positive infinity.</returns>
    public double[,] ToDistanceMatrix()
    {
        int numberOfVertices = Vertices.Count;
        var distanceMatrix = ToAdjacencyMatrix(
            sameVertex: 0.0,
            defaultValue: double.PositiveInfinity
        );

        for (int k = 0; k < numberOfVertices; k++) // Floyd-Warshall algorithm iterates through each vertex as an intermediate point
        {
            for (int i = 0; i < numberOfVertices; i++)
            {
                for (int j = 0; j < numberOfVertices; j++)
                {
                    double throughK = distanceMatrix[i, k] + distanceMatrix[k, j];

                    if (throughK < distanceMatrix[i, j])
                        distanceMatrix[i, j] = throughK;
                }
            }
        }

        return distanceMatrix;
    }

    /// <summary>
    /// Returns the set of vertices that are reachable from the specified vertex via directed paths in the graph.
    /// </summary>
    /// <remarks>The transitive closure includes all vertices that are accessible from the given vertex by
    /// following one or more edges. This method does not modify the graph.</remarks>
    /// <param name="vertex">The vertex from which to compute the transitive closure. Must be present in the graph.</param>
    /// <returns>A list of vertices that can be reached from the specified vertex. The list excludes the input vertex itself and
    /// will be empty if no other vertices are reachable.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified vertex is not found in the graph.</exception>
    public List<Vertex> VertexTransitiveClosure(Vertex vertex)
    {
        int vertexIndex = Vertices.IndexOf(vertex);
        if (vertexIndex == -1)
            throw new ArgumentException("Vertex not found in graph.");

        var numberOfVertices = Vertices.Count;
        var reachabilityMatrix = ToReachabilityMatrix();
        var closure = new List<Vertex>();
        var row = MatrixHelpers.GetMatrixRow<double>(reachabilityMatrix, vertexIndex);

        for (int i = 0; i < numberOfVertices; i++)
        {
            if (row[i] == 1.0 && i != vertexIndex)
            {
                closure.Add(Vertices[i]);
            }
        }

        return closure;
    }

    /// <summary>
    /// Returns a list of vertices that are directly reachable from the specified vertex in the graph.
    /// </summary>
    /// <remarks>This method considers only direct reachability, not indirect paths. The returned list
    /// excludes the input vertex itself.</remarks>
    /// <param name="vertex">The vertex from which to determine direct transitive closure. Must be present in the graph.</param>
    /// <returns>A list of vertices that can be reached directly from the specified vertex. The list will be empty if no vertices
    /// are directly reachable.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified vertex is not found in the graph.</exception>
    public List<Vertex> VertexDirectTransitiveClosure(Vertex vertex)
    {
        int vertexIndex = Vertices.IndexOf(vertex);
        if (vertexIndex == -1)
            throw new ArgumentException("Vertex not found in graph.");

        var numberOfVertices = Vertices.Count;
        var adjacencyMatrix = ToAdjacencyMatrix();
        var closure = new List<Vertex>();
        var row = MatrixHelpers.GetMatrixRow<double>(adjacencyMatrix, vertexIndex);

        for (int i = 0; i < numberOfVertices; i++)
        {
            if (row[i] == 1.0 && i != vertexIndex)
            {
                closure.Add(Vertices[i]);
            }
        }

        return closure;
    }

    /// <summary>
    /// Returns a list of vertices that can reach the specified vertex via directed paths in the graph.
    /// </summary>
    /// <remarks>This method identifies all vertices from which the specified vertex is reachable, excluding
    /// the vertex itself. The result does not include the input vertex even if it is reachable from itself.</remarks>
    /// <param name="vertex">The vertex for which to compute the inverse transitive closure. Must be present in the graph.</param>
    /// <returns>A list of vertices that have a directed path to the specified vertex. The list is empty if no vertices can reach
    /// the given vertex.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified vertex is not found in the graph.</exception>
    public List<Vertex> VertexInverseTransitiveClosure(Vertex vertex)
    {
        int vertexIndex = Vertices.IndexOf(vertex);
        if (vertexIndex == -1)
            throw new ArgumentException("Vertex not found in graph.");

        var numberOfVertices = Vertices.Count;
        var reachabilityMatrix = ToReachabilityMatrix();
        var closure = new List<Vertex>();
        var column = MatrixHelpers.GetMatrixColumn<double>(reachabilityMatrix, vertexIndex);

        for (int i = 0; i < numberOfVertices; i++)
        {
            if (column[i] == 1.0 && i != vertexIndex)
            {
                closure.Add(Vertices[i]);
            }
        }

        return closure;
    }

    /// <summary>
    /// Identifies all strongly connected components in the directed graph using Tarjan's algorithm.
    /// </summary>
    /// <remarks>A strongly connected component is a maximal subset of vertices such that every vertex is
    /// reachable from every other vertex in the subset. The order of components and vertices within each component is
    /// determined by the traversal of the graph and may not be sorted. This method is suitable for use with graphs
    /// containing cycles and can be called multiple times without side effects.</remarks>
    /// <returns>A list of strongly connected components, where each component is represented as a list of vertices. If the graph
    /// contains no strongly connected components, the list will be empty.</returns>
    public List<List<Vertex>> GetStronglyConnectedComponents()
    {
        // TODO: Melhorar isso daqui

        // implement Tarjan's algorithm for finding strongly connected components in directed graphs
        var adjacencyList = ToAdjacencyList(); // your existing adjacency list
        var index = 0;
        var indices = new Dictionary<Vertex, int>();
        var lowLink = new Dictionary<Vertex, int>();
        var onStack = new HashSet<Vertex>();
        var stack = new Stack<Vertex>();
        var result = new List<List<Vertex>>();

        void StrongConnect(Vertex v)
        {
            indices[v] = index;
            lowLink[v] = index;
            index++;
            stack.Push(v);
            onStack.Add(v);

            foreach (var neighbor in adjacencyList[v].Select(edge => edge.To))
            {
                if (!indices.ContainsKey(neighbor))
                {
                    StrongConnect(neighbor);
                    lowLink[v] = Math.Min(lowLink[v], lowLink[neighbor]);
                }
                else if (onStack.Contains(neighbor))
                {
                    lowLink[v] = Math.Min(lowLink[v], indices[neighbor]);
                }
            }

            // If v is a root node, pop the stack and generate an SCC
            if (lowLink[v] == indices[v])
            {
                var scc = new List<Vertex>();
                Vertex w;
                do
                {
                    w = stack.Pop();
                    onStack.Remove(w);
                    scc.Add(w);
                } while (w != v);
                result.Add(scc);
            }
        }

        foreach (var vertex in adjacencyList.Keys)
        {
            if (!indices.ContainsKey(vertex))
                StrongConnect(vertex);
        }

        return result;
    }

    // Necessário para procurar o melhor caminho usando algoritmos de busca
    IEnumerable<Vertex> IGraph<Vertex>.GetNeighbors(Vertex vertex)
    {
        List<Vertex> neighbours = [.. ToAdjacencyList()[vertex].Select(edge => edge.To)];
        return neighbours;
    }
}
