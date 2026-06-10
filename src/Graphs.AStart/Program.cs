using System.Text.Json;
using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Builders;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

/// Carrega capitais
// Carrega o arquivo
//string rawCapitais = File.ReadAllText("data/capitais.json");
//string[] capitais = JsonSerializer.Deserialize<string[]>(
//        rawCapitais
//    )!;
string[] capitais = {"Aracajú",
  "Belém",
  "Belo Horizonte",
  "Boa Vista",
  "Brasília",
  "Campo Grande",
  "Cuiabá",
  "Curitiba",
  "Florianópolis",
  "Fortaleza",
  "Goiânia",
  "João Pessoa",
  "Maceió",
  "Manaus",
  "Natal",
  "Palmas",
  "Porto Alegre",
  "Porto Velho",
  "Recife",
  "Rio Branco",
  "Rio de Janeiro",
  "Salvador",
  "São Luis",
  "São Paulo",
  "Teresina",
  "Vitória"
  };

string jsonDistancias = File.ReadAllText("data/distancias.json");
var distancias = JsonSerializer.Deserialize<
    Dictionary<string, Dictionary<string, double>>
>(jsonDistancias);

string jsonDistanciasEmLinhaReta = File.ReadAllText("data/distancias_em_linha_reta.json");
var distanciasEmLinhaReta = JsonSerializer.Deserialize<
    Dictionary<string, Dictionary<string, double>>
>(jsonDistanciasEmLinhaReta);

// Transforma cada capital em um vértice
List<Vertex> capitaisVertices = ;

// Transforma as distâncias em arestas
List<Edge> distanciasArestas =;

// Transforma as distâncias em linha reta em arestas
List<Edge> distanciasEmLinhaArestas =;

/// Cenário normal: utilizando os pesos proporcionados.

// Grafo com distâncias
GraphBuilder graphBuilder = GraphBuilder.Empty();

graphBuilder.WithEdges(distanciasArestas);
graphBuilder.WithVertices(capitaisVertices);
graphBuilder.WithGraphDirection(GraphDirection.Undirected);
graphBuilder.WithGraphFeatures(GraphFeatures.Weighted);

// Lista de Abertos
// Lista de Fechados

// Grafo com distâncias em linha reta

GraphBuilder graphBuilder = GraphBuilder.Empty();

graphBuilder.WithEdges(distanciasArestas);
graphBuilder.WithVertices(capitaisVertices);
graphBuilder.WithGraphDirection(GraphDirection.Undirected);
graphBuilder.WithGraphFeatures(GraphFeatures.Weighted);

// Lista de Abertos
// Lista de Fechados

// Pega capitais como vertices alatpriamente
Vertex start =;
Vertex end =;
PathFindingAlgorithm<Vertex> algoritmoAStar = new AStar();

// Rota obtida através do algoritmo A* para distância
List<Vertex> rotaDistancia = AStar.Find();

// Rota obtida através do algoritmo A* para distância em linha reta

/// Cenário de congestionamento: escolha trechos por onde o seu algoritmo normalmente passaria e simule cenários de congestionamento aumentando o peso das arestas
// Grafo
// Lista de Abertos
// Lista de Fechados

/// Comparação das rotas obtidas, destacando em quais pontos o “trânsito” força o algoritmo a seguir um caminho diferente.
