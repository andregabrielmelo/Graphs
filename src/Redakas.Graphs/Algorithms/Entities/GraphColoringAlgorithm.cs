using Redakas.Graphs.Entities;

namespace Redakas.Graphs.Algorithms.Entities;

public class GraphColoringAlgorithm : Algorithm<Graph, IEnumerable<ColoringStep>>
{
    public override string Name => "Graph Coloring (DSATUR)";
    public override string Description => "Heurística de coloração de vértices por grau de saturação.";

    public override List<ColoringStep> Apply(Graph graph) => ColorGraph(graph);

    public List<ColoringStep> ColorGraph(Graph graph)
    {
        var vertices = graph.Vertices;
        int vertexCount = vertices.Count;

        var adjacencyMatrix = BuildIntMatrix(graph);
        int[] vertexColors = new int[vertexCount];
        int[] degrees = CalcularGrauVertices(adjacencyMatrix);
        var steps = new List<ColoringStep>();

        // Inicialização: vértice com maior grau recebe cor 1
        int indiceVerticeInicial = 0;
        for (int i = 1; i < vertexCount; i++)
            if (degrees[i] > degrees[indiceVerticeInicial])
                indiceVerticeInicial = i;

        vertexColors[indiceVerticeInicial] = 1;
        steps.Add(new ColoringStep(vertices[indiceVerticeInicial], 1, 1));

        // Iteração: enquanto houver vértice não colorido
        while (vertexColors.Any(cor => cor == 0))
        {
            int[] saturacao = CalcularSaturacaoVertices(adjacencyMatrix, vertexColors);

            int indiceVerticeEscolhido = -1;
            int saturacaoMaisAlta = -1;
            int grauMaisAlto = -1;

            for (int i = 0; i < vertexCount; i++)
            {
                if (vertexColors[i] != 0)
                    continue;

                if (saturacao[i] > saturacaoMaisAlta ||
                    (saturacao[i] == saturacaoMaisAlta && degrees[i] > grauMaisAlto))
                {
                    indiceVerticeEscolhido = i;
                    saturacaoMaisAlta = saturacao[i];
                    grauMaisAlto = degrees[i];
                }
            }

            int corValida = ObterCorValidaParaVertice(adjacencyMatrix, vertexColors, indiceVerticeEscolhido);
            vertexColors[indiceVerticeEscolhido] = corValida;
            steps.Add(new ColoringStep(vertices[indiceVerticeEscolhido], corValida, steps.Count + 1));
        }

        return steps;
    }

    private static int[,] BuildIntMatrix(Graph graph)
    {
        int vertexCount = graph.Vertices.Count;
        double[,] doubleMatrix = graph.ToAdjacencyMatrix(sameVertex: 0.0, defaultValue: 0.0);
        int[,] matrix = new int[vertexCount, vertexCount];

        for (int i = 0; i < vertexCount; i++)
            for (int j = 0; j < vertexCount; j++)
                matrix[i, j] = doubleMatrix[i, j] != 0.0 ? 1 : 0;

        return matrix;
    }

    private static int[] CalcularGrauVertices(int[,] adjacencyMatrix)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);
        int[] degrees = new int[vertexCount];

        for (int i = 0; i < vertexCount; i++)
            for (int j = 0; j < vertexCount; j++)
                if (adjacencyMatrix[i, j] == 1)
                    degrees[i]++;

        return degrees;
    }

    private static int[] CalcularSaturacaoVertices(int[,] adjacencyMatrix, int[] vertexColors)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);
        int[] saturation = new int[vertexCount];

        for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        {
            if (vertexColors[vertexIndex] != 0)
                continue;

            var adjacentColors = new HashSet<int>(); // resetado por vértice

            for (int neighborIndex = 0; neighborIndex < vertexCount; neighborIndex++)
                if (adjacencyMatrix[vertexIndex, neighborIndex] == 1 && vertexColors[neighborIndex] != 0)
                    adjacentColors.Add(vertexColors[neighborIndex]);

            saturation[vertexIndex] = adjacentColors.Count;
        }

        return saturation;
    }

    private static int ObterCorValidaParaVertice(int[,] adjacencyMatrix, int[] vertexColors, int vertexIndex)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);
        var coresUsadas = new HashSet<int>();

        for (int neighborIndex = 0; neighborIndex < vertexCount; neighborIndex++)
            if (adjacencyMatrix[vertexIndex, neighborIndex] == 1 && vertexColors[neighborIndex] != 0)
                coresUsadas.Add(vertexColors[neighborIndex]);

        int cor = 1;
        while (coresUsadas.Contains(cor))
            cor++;

        return cor;
    }
}
