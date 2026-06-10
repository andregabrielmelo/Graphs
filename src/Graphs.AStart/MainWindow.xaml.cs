using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

namespace Graphs.AStart;

public partial class MainWindow : Window
{
    private Graph _graph = new([], [], GraphDirection.Undirected, GraphFeatures.Weighted);

    private List<Vertex> _normalPath = [];
    private List<Vertex> _trafficPath = [];

    private int _currentStep = 0;
    private bool _trafficMode = false;

    private readonly Dictionary<Vertex, Point> _vertexPositions = new();

    private readonly Dictionary<Edge, double> _originalWeights = new();

    private Vertex? _origin;
    private Vertex? _destination;

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
        _graph = new Graph(
            [],
            [],
            GraphDirection.Undirected,
            GraphFeatures.Weighted
        );

        // TODO:
        // Ler distancias.json
        // Criar vertices
        // Criar arestas
        // Salvar pesos originais

        statusText.Text = "Grafo carregado.";
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

    private void NextStep_Click(object sender, RoutedEventArgs e)
    {
        statusText.Text =
            $"Executando passo {_currentStep + 1} do A*";

        _currentStep++;
    }

    private void RunAll_Click(object sender, RoutedEventArgs e)
    {
        StartSearch();

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

        // TODO:
        // Executar algoritmo A*
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _currentStep = 0;

        openList.Items.Clear();
        closedList.Items.Clear();

        statusText.Text = "";
    }

    private void GenerateTraffic_Click(
    object sender,
    RoutedEventArgs e)
    {
        Random random = new();

        for (int i = 0; i < _graph.Edges.Count; i++)
        {
            if (random.NextDouble() < 0.25)
            {
                Edge edge = _graph.Edges[i];

                double factor =
                    random.Next(2, 6);

                _graph.Edges[i] =
                    edge with
                    {
                        Weight = edge.Weight * factor
                    };
            }
        }

        _trafficMode = true;

        statusText.Text =
            "Congestionamento gerado.";
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
            trafficRouteText.Text = route;
        else
            normalRouteText.Text = route;
    }

    private void UpdateComparison()
    {
        comparisonText.Text =
    $"""
Rota normal:
{normalRouteText.Text}

Rota congestionada:
{trafficRouteText.Text}
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
        double circleRadius = Math.Min(canvasCenterX, canvasCenterY) * 0.75;
        const double ellipseSize = 40;

        var vertexPositions = new Dictionary<Vertex, Point>();

        for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        {
            double angle = 2 * Math.PI * vertexIndex / vertexCount;
            double vertexX = canvasCenterX + circleRadius * Math.Cos(angle);
            double vertexY = canvasCenterY + circleRadius * Math.Sin(angle);
            vertexPositions[_graph.Vertices[vertexIndex]] = new Point(vertexX, vertexY);
        }

        foreach (var edge in _graph.Edges)
        {
            Point edgeStart = vertexPositions[edge.From];
            Point edgeEnd = vertexPositions[edge.To];

            graphCanvas.Children.Add(new Line
            {
                X1 = edgeStart.X,
                Y1 = edgeStart.Y,
                X2 = edgeEnd.X,
                Y2 = edgeEnd.Y,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 2
            });

            if (_graph.Direction == GraphDirection.Directed)
            {
                double edgeAngle = Math.Atan2(edgeEnd.Y - edgeStart.Y, edgeEnd.X - edgeStart.X);
                const double arrowLength = 14;
                const double arrowAngle = Math.PI / 7; // ~25.7°

                // Place arrowhead tip at the vertex border (not its center)
                double tipX = edgeEnd.X - (ellipseSize / 2) * Math.Cos(edgeAngle);
                double tipY = edgeEnd.Y - (ellipseSize / 2) * Math.Sin(edgeAngle);

                double base1X = tipX - arrowLength * Math.Cos(edgeAngle - arrowAngle);
                double base1Y = tipY - arrowLength * Math.Sin(edgeAngle - arrowAngle);
                double base2X = tipX - arrowLength * Math.Cos(edgeAngle + arrowAngle);
                double base2Y = tipY - arrowLength * Math.Sin(edgeAngle + arrowAngle);

                graphCanvas.Children.Add(new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(tipX,   tipY),
                        new Point(base1X, base1Y),
                        new Point(base2X, base2Y)
                    },
                    Fill = Brushes.DarkGray,
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 2
                });
            }
        }

        foreach (var vertex in _graph.Vertices)
        {
            Point vertexCenter = vertexPositions[vertex];
            Brush fillBrush = Brushes.LightBlue;

            var ellipse = new Ellipse
            {
                Width = ellipseSize,
                Height = ellipseSize,
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, vertexCenter.X - ellipseSize / 2);
            Canvas.SetTop(ellipse, vertexCenter.Y - ellipseSize / 2);
            graphCanvas.Children.Add(ellipse);

            var label = new TextBlock
            {
                Text = vertex.Name,
                FontWeight = FontWeights.Bold,
                Width = ellipseSize,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(label, vertexCenter.X - ellipseSize / 2);
            Canvas.SetTop(label, vertexCenter.Y - ellipseSize / 2 + 11);
            graphCanvas.Children.Add(label);
        }
    }
}
