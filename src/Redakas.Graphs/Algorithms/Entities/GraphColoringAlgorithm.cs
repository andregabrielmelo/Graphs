using Redakas.Graphs.Interfaces;

namespace Redakas.Graphs.Algorithms.Entities;

public abstract class GraphColoringAlgorithm<T> : Algorithm<IGraph graph, IEnumerable<T>>
    where T : notnull
{
    public override List<T> Apply(IGraph value)
    {
        int[] colors = colorirGrafo(value);

        throw NotImplementedException();
    }

    int[] colorirGrafo(int[,] graphAdjacencyMatrix, string[] vertices)
    {
        int quantidadeVertices = graphAdjacencyMatrix.GetLength(0);

        // Arrays que vão ser usados para guardar as cores, graus e saturação de cada vertice
        int[] verticesCores = new int[quantidadeVertices];
        int[] verticesGraus = new int[quantidadeVertices];
        int[] verticesSaturacao = new int[quantidadeVertices];
        int[] cores = [];

        // Enquanto todos os vertices não forem coloridos
        while (verticesCores.Min() == 0)
        {
            verticesGraus = calcularGrauVertices(graphAdjacencyMatrix);
            verticesSaturacao = calcularSaturacaoVertices(graphAdjacencyMatrix, verticesCores);

            // Pega o primeiro vértice com a maior saturação e maior grau
            int saturacaoMaisAlta = verticesSaturacao.Max();
            int indiceVerticeEscolhido;
            int grauMaisAlto = 0;
            for (int i = 0; i < quantidadeVertices; i++)
            {
                if (verticesSaturacao[i] == saturacaoMaisAlta && verticesGraus[i] > grauMaisAlto)
                {
                    indiceVerticeEscolhido = i;
                    grauMaisAlto = verticesGraus[i];
                }
            }

            int corValidaParaVertice = obterCorValidaParaVertice(graphAdjacencyMatrix, verticesCores, indic);

            verticesCores[indiceVerticeEscolhido] = corValidaParaVertice;

        }

    }

    int[] calcularGrauVertices(int[,] graphAdjacencyMatrix)
    {
        int quantityOfVertices = graphAdjacencyMatrix.GetLength(0);

        int[] verticesDegress = new int[quantityOfVertices];

        // Calcula o grau de cada vértice, somando os vertices adjacentes 
        for (int i = 0; i < quantityOfVertices; i++)
        {
            int degree = 0;
            for (int j = 0; j < quantityOfVertices; j++)
            {
                if (graphAdjacencyMatrix[i, j] == 1)
                {
                    degree++;
                }
            }
            verticesDegress[i] = degree;
        }

        return verticesDegress;
    }

    int[] calcularSaturacaoVertices(int[,] graphAdjacencyMatrix, int[] verticesColors)
    {
        int quantityOfVertices = graphAdjacencyMatrix.GetLength(0);

        int[] verticesSaturation = new int[quantityOfVertices];
        HashSet<int> adjacentColors = [];

        // Para cada vertice
        for (int vertexIndex = 0; vertexIndex < quantityOfVertices; vertexIndex++)
        {
            for (int adjacentVertexIndex = 0; adjacentVertexIndex < quantityOfVertices; adjacentVertexIndex++)
            {
                // Pula o mesmo vértice
                if (vertexIndex == adjacentVertexIndex)
                {
                    continue;
                }

                // Se o vertex é adjacente e tem uma cor, aumenta o grau de saturação
                bool isVertexAdjacent = graphAdjacencyMatrix[vertexIndex, adjacentVertexIndex] == 1;
                bool isVertexColored = verticesColors[adjacentVertexIndex] != 0;
                if (isVertexAdjacent && isVertexColored)
                {
                    adjacentColors.Add(verticesColors[adjacentVertexIndex]);
                }
            }

            verticesSaturation[vertexIndex] = adjacentColors.Count;
        }

        return verticesSaturation;
    }

    int obterCorValidaParaVertice(int[,] graphAdjacencyMatrix, int[] verticesColors, int indiceVertice)
    {
        int quantityOfVertices = graphAdjacencyMatrix.GetLength(0);

        // Pega a primeira cor que não está em um vértice adjacente
        for (int i = 0; i < quantityOfVertices; i++)
        {

        }
    }
}