namespace Redakas.Graphs.Entities;

/// <summary>
/// Snapshot de uma iteração do A*: o nó fechado nesta etapa, seus custos
/// (F = G + H) e o estado das listas aberta/fechada após o fechamento.
/// Replica o formato de acompanhamento dos slides (Abertos/Fechados por etapa).
/// </summary>
public record AStarStep(
    Vertex Fechado,
    double G,
    double H,
    double F,
    List<Vertex> Abertos,
    List<Vertex> Fechados
)
{
    public override string ToString() =>
        $"Fecha {Fechado.Name} (G={G:0.#} H={H:0.#} F={F:0.#}) | " +
        $"Abertos: [{string.Join(", ", Abertos.Select(v => v.Name))}] | " +
        $"Fechados: [{string.Join(", ", Fechados.Select(v => v.Name))}]";
}
