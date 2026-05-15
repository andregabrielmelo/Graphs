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

        // Constrói a matriz de adjacência do grafo.
        // Se existir conexão entre dois vértices:
        // 1 = conectado
        // 0 = não conectado
        var adjacencyMatrix = BuildIntMatrix(graph);

        // Vetor que armazena a cor de cada vértice.
        // 0 significa "não colorido".
        int[] vertexColors = new int[vertexCount];

        // Calcula o grau de cada vértice.
        // Grau = quantidade de vizinhos conectados.
        int[] degrees = CalcularGrauVertices(adjacencyMatrix);

        // Lista utilizada para armazenar o passo a passo
        // da coloração do grafo.
        var steps = new List<ColoringStep>();

        // =========================================================
        // ETAPA 1 — ESCOLHER O PRIMEIRO VÉRTICE
        // =========================================================
        // O algoritmo DSATUR começa colorindo o vértice
        // com maior grau, pois ele possui mais conexões
        // e tende a impactar mais o restante do grafo.
        int indiceVerticeInicial = 0;
        for (int i = 1; i < vertexCount; i++)
            if (degrees[i] > degrees[indiceVerticeInicial])
                indiceVerticeInicial = i;

        // Atribui a primeira cor ao vértice inicial.
        vertexColors[indiceVerticeInicial] = 1;

        steps.Add(new ColoringStep(vertices[indiceVerticeInicial], 1, 1));

        // =========================================================
        // ETAPA 2 — COLORIR OS DEMAIS VÉRTICES
        // =========================================================
        // Continua enquanto existir vértice sem cor.
        while (vertexColors.Any(cor => cor == 0))
        {
            // Calcula o grau de saturação de cada vértice.
            //
            // Grau de saturação = quantidade de cores diferentes
            // presentes nos vértices vizinhos.
            //
            // Quanto maior a saturação, mais restrito o vértice está.
            int[] saturacao = CalcularSaturacaoVertices(adjacencyMatrix, vertexColors);

            int indiceVerticeEscolhido = -1;
            int saturacaoMaisAlta = -1;
            int grauMaisAlto = -1;

            // =====================================================
            // ESCOLHA DO PRÓXIMO VÉRTICE
            // =====================================================
            // O DSATUR escolhe:
            //
            // 1. O vértice com MAIOR saturação.
            // 2. Em caso de empate, escolhe o de MAIOR grau.
            //
            // Isso reduz a chance de precisar criar
            // muitas cores diferentes no futuro.
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

            // =====================================================
            // DEFINIÇÃO DA COR
            // =====================================================
            // Busca a menor cor possível que NÃO esteja
            // sendo usada pelos vizinhos do vértice.
            //
            // Exemplo:
            // Se vizinhos usam cores {1,2},
            // então a próxima cor válida será 3.
            int corValida = ObterCorValidaParaVertice(adjacencyMatrix, vertexColors, indiceVerticeEscolhido);

            // Aplica a cor encontrada.
            vertexColors[indiceVerticeEscolhido] = corValida;

            // Registra o passo da coloração.
            steps.Add(new ColoringStep(vertices[indiceVerticeEscolhido], corValida, steps.Count + 1));
        }

        return steps;
    }

    private static int[] CalcularGrauVertices(double[,] adjacencyMatrix)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);
        int[] degrees = new int[vertexCount];

        for (int i = 0; i < vertexCount; i++)
            for (int j = 0; j < vertexCount; j++)
                if (adjacencyMatrix[i, j] == 1)
                    degrees[i]++;

        return degrees;
    }

    private static int[] CalcularSaturacaoVertices(double[,] adjacencyMatrix, int[] vertexColors)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);
        int[] saturation = new int[vertexCount];

        for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        {
            // Ignora vértices já coloridos.
            if (vertexColors[vertexIndex] != 0)
                continue;

            // Guarda as cores encontradas nos vizinhos.
            var adjacentColors = new HashSet<int>(); // resetado por vértice

            // Percorre os vizinhos do vértice atual.
            for (int neighborIndex = 0; neighborIndex < vertexCount; neighborIndex++)
                // Se existe aresta E o vizinho já possui cor,
                // adiciona essa cor ao conjunto.
                if (adjacencyMatrix[vertexIndex, neighborIndex] == 1 && vertexColors[neighborIndex] != 0)
                    adjacentColors.Add(vertexColors[neighborIndex]);

            // O grau de saturação é a quantidade
            // de cores diferentes nos vizinhos.
            saturation[vertexIndex] = adjacentColors.Count;
        }

        return saturation;
    }

    private static int ObterCorValidaParaVertice(double[,] adjacencyMatrix, int[] vertexColors, int vertexIndex)
    {
        int vertexCount = adjacencyMatrix.GetLength(0);

        // Guarda as cores já utilizadas pelos vizinhos.
        var coresUsadas = new HashSet<int>();

        // Descobre quais cores os vizinhos estão usando.
        for (int neighborIndex = 0; neighborIndex < vertexCount; neighborIndex++)
            if (adjacencyMatrix[vertexIndex, neighborIndex] == 1 && vertexColors[neighborIndex] != 0)
                coresUsadas.Add(vertexColors[neighborIndex]);

        // Começa tentando usar a cor 1.
        int cor = 1;

        // Enquanto a cor estiver sendo usada
        // por algum vizinho, tenta a próxima.
        while (coresUsadas.Contains(cor))
            cor++;

        // Retorna a menor cor válida encontrada.
        return cor;
    }

    private static double[,] BuildIntMatrix(Graph graph)
    {
        var matrix = graph.ToAdjacencyMatrix(sameVertex: 0.0, defaultValue: 0.0);
        int n = graph.Vertices.Count;

        // Treat directed edges as bidirectional for coloring conflict purposes.
        // If there's an edge from i to j, also mark j to i so the adjacency
        // relationship is symmetric (both vertices are considered adjacent).
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (matrix[i, j] != 0)
                    matrix[j, i] = matrix[i, j];

        return matrix;
    }
}
