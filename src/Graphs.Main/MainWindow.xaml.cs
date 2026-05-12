using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Builders;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Enums;

namespace Graphs.Main;

public partial class MainWindow : Window
{
    private Graph _graph = new Graph([], [], GraphDirection.Undirected, GraphFeatures.None);
    private List<ColoringStep>? _steps = null;
    private int _currentStep = 0;
    private readonly Dictionary<Vertex, int> _vertexColors = new();

    private static readonly Dictionary<int, Brush> ColorPalette = new()
    {
        { 0, Brushes.LightGray },
        { 1, Brushes.Tomato },
        { 2, Brushes.CornflowerBlue },
        { 3, Brushes.MediumSeaGreen },
        { 4, Brushes.Gold },
        { 5, Brushes.MediumOrchid },
        { 6, Brushes.DarkOrange },
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => DrawGraph();

    private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawGraph();

    // ── Desenho ──────────────────────────────────────────────────────────────

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
                X1 = edgeStart.X, Y1 = edgeStart.Y,
                X2 = edgeEnd.X,   Y2 = edgeEnd.Y,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 2
            });
        }

        foreach (var vertex in _graph.Vertices)
        {
            Point vertexCenter = vertexPositions[vertex];
            int color = _vertexColors.TryGetValue(vertex, out int storedColor) ? storedColor : 0;
            Brush fillBrush = GetColorBrush(color);

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

    private static Brush GetColorBrush(int color)
    {
        if (ColorPalette.TryGetValue(color, out Brush? brush))
            return brush;

        double hue = (color * 137.5) % 360;
        return new SolidColorBrush(HslToRgb(hue, 0.65, 0.55));
    }

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

    // ── Gerenciamento do grafo ─────────────────────────────────────────────

    private void AddVertex_Click(object sender, RoutedEventArgs e)
    {
        string name = vertexNameInput.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            statusText.Text = "Digite o nome do vértice.";
            return;
        }

        if (_graph.Vertices.Any(v => v.Name == name))
        {
            statusText.Text = $"Vértice '{name}' já existe.";
            return;
        }

        _graph.AddVertex(new Vertex(name));
        vertexNameInput.Clear();
        statusText.Text = string.Empty;
        ResetColoring();
        DrawGraph();
    }

    private void RemoveVertex_Click(object sender, RoutedEventArgs e)
    {
        string name = vertexNameInput.Text.Trim();
        var vertex = _graph.Vertices.FirstOrDefault(v => v.Name == name);

        if (vertex is null)
        {
            statusText.Text = $"Vértice '{name}' não encontrado.";
            return;
        }

        _graph.RemoveVertex(vertex);
        vertexNameInput.Clear();
        statusText.Text = string.Empty;
        ResetColoring();
        DrawGraph();
    }

    private void AddEdge_Click(object sender, RoutedEventArgs e)
    {
        var from = _graph.Vertices.FirstOrDefault(v => v.Name == edgeFromInput.Text.Trim());
        var to   = _graph.Vertices.FirstOrDefault(v => v.Name == edgeToInput.Text.Trim());

        if (from is null || to is null)
        {
            statusText.Text = "Um ou ambos os vértices não existem.";
            return;
        }

        if (_graph.HasEdge(from, to))
        {
            statusText.Text = "Aresta já existe.";
            return;
        }

        _graph.AddEdge(new Edge(from, to));
        edgeFromInput.Clear();
        edgeToInput.Clear();
        statusText.Text = string.Empty;
        ResetColoring();
        DrawGraph();
    }

    private void RemoveEdge_Click(object sender, RoutedEventArgs e)
    {
        var from = _graph.Vertices.FirstOrDefault(v => v.Name == edgeFromInput.Text.Trim());
        var to   = _graph.Vertices.FirstOrDefault(v => v.Name == edgeToInput.Text.Trim());

        if (from is null || to is null || !_graph.HasEdge(from, to))
        {
            statusText.Text = "Aresta não encontrada.";
            return;
        }

        _graph.RemoveEdge(_graph.Edges.First(edge => edge.From == from && edge.To == to));
        edgeFromInput.Clear();
        edgeToInput.Clear();
        statusText.Text = string.Empty;
        ResetColoring();
        DrawGraph();
    }

    // ── Coloração passo a passo ────────────────────────────────────────────

    private void NextStep_Click(object sender, RoutedEventArgs e) => ApplyNextStep();

    private void RunAll_Click(object sender, RoutedEventArgs e)
    {
        EnsureStepsCalculated();
        if (_steps is null) return;

        while (_currentStep < _steps.Count)
            ApplyNextStep();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ResetColoring();
        DrawGraph();
    }

    private void ApplyNextStep()
    {
        if (_graph.Vertices.Count == 0)
        {
            statusText.Text = "Adicione vértices antes de colorir.";
            return;
        }

        EnsureStepsCalculated();
        if (_steps is null) return;

        if (_currentStep >= _steps.Count)
        {
            statusText.Text = $"Coloração concluída. Número cromático: {_steps.Max(s => s.Color)}";
            return;
        }

        ColoringStep step = _steps[_currentStep];
        _vertexColors[step.Vertex] = step.Color;
        coloringSequenceList.Items.Add($"Passo {step.StepNumber}: {step.Vertex.Name} → Cor {step.Color}");
        coloringSequenceList.ScrollIntoView(coloringSequenceList.Items[^1]);

        _currentStep++;
        DrawGraph();

        if (_currentStep >= _steps.Count)
            statusText.Text = $"Coloração concluída. Número cromático: {_steps.Max(s => s.Color)}";
    }

    private void EnsureStepsCalculated()
    {
        if (_steps is not null) return;

        var algorithm = new GraphColoringAlgorithm();
        _steps = algorithm.ColorGraph(_graph);
    }

    private void ResetColoring()
    {
        _steps = null;
        _currentStep = 0;
        _vertexColors.Clear();
        coloringSequenceList.Items.Clear();
        statusText.Text = string.Empty;
    }
}
