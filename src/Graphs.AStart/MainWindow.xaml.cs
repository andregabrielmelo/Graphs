using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graphs.AStart;

public partial class MainWindow : Window
{
    private static readonly string[] _capitais =
    [
        "Aracaju",
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
        "São Luís",
        "São Paulo",
        "Teresina",
        "Vitória"
    ];

    private Graph _graph = new([], [], GraphDirection.Undirected, GraphFeatures.Weighted);
    private Graph _graphWithDefaultWeights = new([], [], GraphDirection.Undirected, GraphFeatures.Weighted);

    private List<Vertex> _normalPath = [];
    private List<Vertex> _trafficPath = [];

    private int _currentStep = 0;
    private bool _trafficMode = false;

    private readonly Dictionary<Edge, double> _originalWeights = new();

    private Dictionary<string, Vertex> _verticesByName = new();
    private Dictionary<string, Dictionary<string, double>> _heuristicDistances = new();
    private readonly HashSet<Edge> _trafficEdges = [];

    private Vertex? _origin;
    private Vertex? _destination;

    private List<AStarStep> _steps = [];

    private double _normalCost;
    private double _trafficCost;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadGraph();
        PopulateCapitalCombos();
        DrawGraph();
    }

    private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawGraph();

    private void LoadGraph()
    {
        string jsonDistancias =
            File.ReadAllText("data/distancias.json");

        var distancias =
            JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, double>>
            >(jsonDistancias)!;

        string jsonDistanciasEmLinhaReta =
            File.ReadAllText("data/distancias_em_linha_reta.json");

        var distanciasEmLinhaReta =
            JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, double>>
            >(jsonDistanciasEmLinhaReta)!;

        _heuristicDistances = distanciasEmLinhaReta;

        // Cria os vértices
        _verticesByName.Clear();
        foreach (string capital in _capitais)
        {
            var vertex = new Vertex(capital);

            _verticesByName[capital] = vertex;
        }

        // Cria as arestas
        List<Edge> edges = [];
        foreach (var origem in distancias)
        {
            string cidadeOrigem = origem.Key;

            foreach (var destino in origem.Value)
            {
                string cidadeDestino = destino.Key;
                double distancia = destino.Value;

                if (cidadeOrigem == cidadeDestino)
                    continue;

                Vertex from = _verticesByName[cidadeOrigem];
                Vertex to = _verticesByName[cidadeDestino];

                edges.Add(
                    new Edge(
                        from,
                        to,
                        distancia
                    )
                );
            }
        }

        // Cria o grafo
        _graph = new Graph(
            _verticesByName.Values,
            edges,
            GraphDirection.Undirected,
            GraphFeatures.Weighted
        );
        
        _graphWithDefaultWeights = new Graph(
            _verticesByName.Values,
            edges,
            GraphDirection.Undirected,
            GraphFeatures.Weighted
        );

        // Guarda pesos originais
        _originalWeights.Clear();
        foreach (var edge in _graph.Edges)
        {
            if (!_originalWeights.ContainsKey(edge))
                _originalWeights[edge] = edge.Weight;
        }

        // Popular comboboxes
        originComboBox.ItemsSource = _capitais;
        destinationComboBox.ItemsSource = _capitais;

        originComboBox.SelectedIndex = 0;
        destinationComboBox.SelectedIndex = 1;

        statusText.Text =
            $"Grafo carregado com {_graph.Vertices.Count} capitais e {_graph.Edges.Count} arestas.";
    }

    private void PopulateCapitalCombos()
    {
        originComboBox.ItemsSource =
            _graph.Vertices.Select(v => v.Name);

        destinationComboBox.ItemsSource =
            _graph.Vertices.Select(v => v.Name);

        if (_graph.Vertices.Count > 1)
        {
            originComboBox.SelectedIndex = 0;
            destinationComboBox.SelectedIndex = 1;
        }
    }

    private void NextStep_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (_steps.Count == 0)
        {
            StartSearch();
        }

        if (_currentStep >= _steps.Count)
        {
            statusText.Text =
                "Todos os passos já foram exibidos.";

            return;
        }

        AStarStep step =
            _steps[_currentStep];

        UpdateOpenList(
            step.Abertos.Select(v =>
                $"{v.Name}")
        );

        UpdateClosedList(
            step.Fechados.Select(v =>
                $"{v.Name}")
        );

        statusText.Text =
            $"Passo {_currentStep + 1}/{_steps.Count} - Fechou {step.Fechado.Name} (F={step.F:0})";

        _currentStep++;
    }

    private void RunAll_Click(
    object sender,
    RoutedEventArgs e)
    {
        StartSearch();

        if (_steps.Count > 0)
        {
            AStarStep last =
                _steps[^1];

            UpdateOpenList(
                last.Abertos.Select(v => v.Name)
            );

            UpdateClosedList(
                last.Fechados.Select(v => v.Name)
            );
        }

        statusText.Text =
            "Busca A* concluída.";
    }

    private void StartSearch()
    {
        if (originComboBox.SelectedItem is null ||
            destinationComboBox.SelectedItem is null)
        {
            MessageBox.Show(
                "Selecione origem e destino."
            );

            return;
        }

        string originName =
            originComboBox.SelectedItem.ToString()!;

        string destinationName =
            destinationComboBox.SelectedItem.ToString()!;

        _origin =
            _graph.Vertices.First(v => v.Name == originName);

        _destination =
            _graph.Vertices.First(v => v.Name == destinationName);

        openList.Items.Clear();
        closedList.Items.Clear();

        // Passe a heuristica a ser utilizada quando se inicializa o algoritimo A*
        AStar algorithm = new AStar(
            (start, end) => _heuristicDistances[start.Name][end.Name]
        );

        _normalPath =
    algorithm.Find(
        _origin,
        _destination,
        _graphWithDefaultWeights
    );

        _steps = algorithm.Steps.ToList();

        _trafficPath =
            algorithm.Find(
                _origin,
                _destination,
                _graph
            );

        _normalCost =
    CalculatePathCost(
        _normalPath,
        _graphWithDefaultWeights
    );

        _trafficCost =
            CalculatePathCost(
                _trafficPath,
                _graph
            );

        ShowPath(_normalPath, false);
        ShowPath(_trafficPath, true);

        UpdateComparison();
        DrawGraph();
    }

    private void Reset_Click(
    object sender,
    RoutedEventArgs e)
    {
        _currentStep = 0;

        _steps.Clear();

        _normalPath.Clear();
        _trafficPath.Clear();

        openList.Items.Clear();
        closedList.Items.Clear();

        statusText.Text = "";

        DrawGraph();
    }

    // TODO: Deveria deixar mais explicito ao usuario para quanto os pesos mudaram
    private void GenerateTraffic_Click(
    object sender,
    RoutedEventArgs e)
    {
        Random random = new();

        _trafficEdges.Clear();

        for (int i = 0; i < _graph.Edges.Count; i++)
        {
            if (random.NextDouble() < 0.25)
            {
                Edge edge = _graph.Edges[i];

                double factor = random.Next(2, 6);

                Edge newEdge =
                    edge with
                    {
                        Weight = edge.Weight * factor
                    };

                _graph.Edges[i] = newEdge;

                _trafficEdges.Add(newEdge);
            }
        }

        _trafficMode = true;

        statusText.Text =
            "Congestionamento gerado.";

        DrawGraph();
    }

    private void RestoreWeights_Click(
    object sender,
    RoutedEventArgs e)
    {
        foreach (var pair in _originalWeights)
        {
            Edge current = pair.Key;

            int index =
                _graph.Edges.IndexOf(current);

            if (index >= 0)
            {
                _graph.Edges[index] =
                    current with
                    {
                        Weight = pair.Value
                    };
            }
        }

        _trafficMode = false;

        statusText.Text =
            "Pesos restaurados.";

        _trafficEdges.Clear();

        DrawGraph();
    }

    private void UpdateOpenList(
    IEnumerable<string> items)
    {
        openList.Items.Clear();

        foreach (string item in items)
            openList.Items.Add(item);
    }

    private void UpdateClosedList(
    IEnumerable<string> items)
    {
        closedList.Items.Clear();

        foreach (string item in items)
            closedList.Items.Add(item);
    }

    private void ShowPath(
    IEnumerable<Vertex> path,
    bool traffic)
    {
        string route =
            string.Join(
                " -> ",
                path.Select(v => v.Name)
            );

        if (traffic)
        {
            trafficRouteText.Text =
                $"{route}\n\nCusto total: {_trafficCost:N0} km";
        }
        else
        {
            normalRouteText.Text =
                $"{route}\n\nCusto total: {_normalCost:N0} km";
        }
    }

    // TODO: Deveria descrever o que levou o algoritimo a tomar a decis'ao diferente e demonstrar as possibilidades no momento de escolhar
    private void UpdateComparison()
    {
        bool routeChanged =
            !_normalPath.SequenceEqual(_trafficPath);

        double difference =
            _trafficCost - _normalCost;

        comparisonText.Text =
    $"""
Rota Normal:
{string.Join(" -> ", _normalPath.Select(v => v.Name))}

Custo:
{_normalCost:N0} km

Rota Congestionada:
{string.Join(" -> ", _trafficPath.Select(v => v.Name))}

Custo:
{_trafficCost:N0} km

Diferença:
{difference:N0} km

Resultado:
{(routeChanged
        ? "O congestionamento forçou o A* a escolher uma rota diferente."
        : "O A* manteve a mesma rota.")}

Aumento de custo:
{((difference / _normalCost) * 100):N1}%
""";
    }

    // ── Desenho ──────────────────────────────────────────────────────────────
    private static Color HslToRgb(double hue, double saturation, double lightness)
    {
        double chroma = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        double huePrime = hue / 60.0;
        double secondComponent = chroma * (1 - Math.Abs(huePrime % 2 - 1));

        double r1, g1, b1;
        if      (huePrime < 1) { r1 = chroma; g1 = secondComponent; b1 = 0; }
        else if (huePrime < 2) { r1 = secondComponent; g1 = chroma; b1 = 0; }
        else if (huePrime < 3) { r1 = 0; g1 = chroma; b1 = secondComponent; }
        else if (huePrime < 4) { r1 = 0; g1 = secondComponent; b1 = chroma; }
        else if (huePrime < 5) { r1 = secondComponent; g1 = 0; b1 = chroma; }
        else                   { r1 = chroma; g1 = 0; b1 = secondComponent; }

        double lightnessAdjust = lightness - chroma / 2;
        return Color.FromRgb(
            (byte)((r1 + lightnessAdjust) * 255),
            (byte)((g1 + lightnessAdjust) * 255),
            (byte)((b1 + lightnessAdjust) * 255)
        );
    }

    private void DrawGraph()
    {
        graphCanvas.Children.Clear();

        int vertexCount = _graph.Vertices.Count;

        if (vertexCount == 0)
            return;

        double canvasCenterX = graphCanvas.ActualWidth / 2;
        double canvasCenterY = graphCanvas.ActualHeight / 2;

        double circleRadius =
            Math.Min(canvasCenterX, canvasCenterY) * 0.75;

        const double ellipseSize = 40;

        Dictionary<Vertex, Point> vertexPositions = [];

        // Posicionamento dos vértices
        for (int i = 0; i < vertexCount; i++)
        {
            double angle =
                2 * Math.PI * i / vertexCount;

            double x =
                canvasCenterX +
                circleRadius * Math.Cos(angle);

            double y =
                canvasCenterY +
                circleRadius * Math.Sin(angle);

            vertexPositions[_graph.Vertices[i]] =
                new Point(x, y);
        }

        // Caminho que será destacado
        HashSet<(Vertex, Vertex)> pathEdges = [];

        List<Vertex> pathToDraw =
            _trafficMode
                ? _trafficPath
                : _normalPath;

        for (int i = 0; i < pathToDraw.Count - 1; i++)
        {
            pathEdges.Add(
                (pathToDraw[i], pathToDraw[i + 1]));

            pathEdges.Add(
                (pathToDraw[i + 1], pathToDraw[i]));
        }

        // Desenha arestas
        foreach (Edge edge in _graph.Edges)
        {
            Point start =
                vertexPositions[edge.From];

            Point end =
                vertexPositions[edge.To];

            bool isPath =
                pathEdges.Contains(
                    (edge.From, edge.To));

            bool isTraffic =
                _trafficEdges.Contains(edge);

            Brush color =
                isPath
                    ? Brushes.LimeGreen
                    : isTraffic
                        ? Brushes.Red
                        : Brushes.DarkGray;

            double thickness =
                isPath
                    ? 5
                    : isTraffic
                        ? 4
                        : 1.5;

            graphCanvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = color,
                StrokeThickness = thickness
            });

            // Peso da aresta
            double middleX =
                (start.X + end.X) / 2;

            double middleY =
                (start.Y + end.Y) / 2;

            graphCanvas.Children.Add(new TextBlock
            {
                Text = ((int)edge.Weight).ToString(),
                FontSize = 10,
                Background = Brushes.White
            });

            Canvas.SetLeft(
                graphCanvas.Children[^1],
                middleX);

            Canvas.SetTop(
                graphCanvas.Children[^1],
                middleY);
        }

        // Desenha vértices
        foreach (Vertex vertex in _graph.Vertices)
        {
            Point center =
                vertexPositions[vertex];

            Brush fill =
                Brushes.LightBlue;

            if (_origin == vertex)
                fill = Brushes.LightGreen;

            if (_destination == vertex)
                fill = Brushes.Orange;

            if (pathToDraw.Contains(vertex))
                fill = Brushes.Gold;

            Ellipse ellipse = new()
            {
                Width = ellipseSize,
                Height = ellipseSize,
                Fill = fill,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Canvas.SetLeft(
                ellipse,
                center.X - ellipseSize / 2);

            Canvas.SetTop(
                ellipse,
                center.Y - ellipseSize / 2);

            graphCanvas.Children.Add(ellipse);

            TextBlock label = new()
            {
                Text = vertex.Name,
                FontWeight = FontWeights.Bold,
                Width = 90,
                TextAlignment = TextAlignment.Center,
                FontSize = 10
            };

            Canvas.SetLeft(
                label,
                center.X - 45);

            Canvas.SetTop(
                label,
                center.Y + 20);

            graphCanvas.Children.Add(label);
        }

        double displayedCost =
    _trafficMode
        ? _trafficCost
        : _normalCost;

        TextBlock costLabel = new()
        {
            Text =
        $"Custo Total: {displayedCost:N0} km",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Background = Brushes.White
        };

        Canvas.SetLeft(costLabel, 10);
        Canvas.SetTop(costLabel, 10);

        graphCanvas.Children.Add(costLabel);
    }

    private static double CalculatePathCost(
    IReadOnlyList<Vertex> path,
    Graph graph)
    {
        if (path.Count < 2)
            return 0;

        double totalCost = 0;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vertex from = path[i];
            Vertex to = path[i + 1];

            Edge? edge =
                graph.Edges.FirstOrDefault(e =>
                    (e.From == from && e.To == to) ||
                    (e.From == to && e.To == from));

            if (edge is not null)
                totalCost += edge.Weight;
        }

        return totalCost;
    }
}
