namespace Redakas.Graphs.Interfaces;

public interface IGraph<TVertex>
{
    List<TVertex> Vertices { get; }

    //IEnumerable<TEdge> Edges { get; }

    IEnumerable<TVertex> GetNeighbors(TVertex vertex);
}
