using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Redakas.Graphs.Algorithms.Entities;
using Redakas.Graphs.Entities;
using Redakas.Graphs.Helpers;

namespace Graphs.Main;

public partial class MainWindow : Window
{
    private Graph _grafoNormal = null!;
    private Dictionary<string, Vertex> _vertices = null!;
    private Func<Vertex, Vertex, double> _heuristica = null!;

    private List<Vertex>? _rotaNormal;
    private double _custoNormal;
    private List<Vertex>? _rotaCongestionada;
    private double _custoCongestionado;
    private List<(Vertex De, Vertex Para)> _trechosCongestionados = [];
    private double _fatorAtual;

    private IReadOnlyList<AStarStep>? _steps;
    private int _currentStep;

    // Contorno do Brasil (longitude, latitude) carregado do GeoJSON
    private List<(double Lon, double Lat)> _contornoBrasil = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CarregarDados();
        PreencherCombos();
        DrawGraph();
    }

    private void CarregarDados()
    {
        // Caminho relativo ao executável (CWD pode ser outro, ex.: dotnet run da raiz)
        string dataDir = System.IO.Path.Combine(AppContext.BaseDirectory, "data");
        var estradas = WeightedMatrixLoader.Parse(
            File.ReadAllText(System.IO.Path.Combine(dataDir, "distancias.json")), CapitaisData.Aliases);
        var linhaReta = WeightedMatrixLoader.Parse(
            File.ReadAllText(System.IO.Path.Combine(dataDir, "distancias_em_linha_reta.json")), CapitaisData.Aliases);

        (_grafoNormal, _vertices) = WeightedMatrixLoader.BuildGraph(estradas);
        _heuristica = WeightedMatrixLoader.BuildHeuristica(linhaReta);

        // Posição geográfica para o desenho do mapa
        foreach (var (nome, vertex) in _vertices)
        {
            if (CapitaisData.Coordenadas.TryGetValue(nome, out var c))
                vertex.Position = new Position(c.Lon, c.Lat);
        }

        CarregarContornoBrasil();
    }

    private void CarregarContornoBrasil()
    {
        // GeoJSON: features[0].geometry.coordinates[0] = anel [[lon, lat], ...]
        string path = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "brasil.geo.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var anel = doc.RootElement
            .GetProperty("features")[0]
            .GetProperty("geometry")
            .GetProperty("coordinates")[0];

        _contornoBrasil = [.. anel.EnumerateArray()
            .Select(p => (p[0].GetDouble(), p[1].GetDouble()))];
    }

    private void PreencherCombos()
    {
        var nomes = _vertices.Keys.OrderBy(n => n).ToList();
        origemCombo.ItemsSource = nomes;
        destinoCombo.ItemsSource = nomes;
        origemCombo.SelectedItem = "São Paulo";
        destinoCombo.SelectedItem = "Fortaleza";
    }

    private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawGraph();

    private void MostrarArestas_Changed(object sender, RoutedEventArgs e)
    {
        // Guard: dispara durante InitializeComponent antes do grafo carregar
        if (_vertices is not null)
            DrawGraph();
    }

    private void Cenario_Changed(object sender, RoutedEventArgs e)
    {
        // Guard: dispara durante InitializeComponent antes dos elementos nomeados existirem
        if (congestionamentoPanel is null) return;

        // Cenário 1 = só rota normal; Cenário 2 = normal + congestionamento
        congestionamentoPanel.Visibility = cenario2Radio.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (_vertices is not null)
        {
            ResetResultados();
            statusText.Text = "";
            DrawGraph();
        }
    }

    // ── Execução dos cenários ────────────────────────────────────────────────

    private void RodarNormal_Click(object sender, RoutedEventArgs e)
    {
        if (!TrySelecionarRota(out var origem, out var destino)) return;

        ResetResultados();

        var astar = new AStar(_heuristica);
        _rotaNormal = astar.Find(origem, destino, _grafoNormal);
        _steps = astar.Steps;
        _currentStep = 0;

        if (_rotaNormal.Count == 0)
        {
            statusText.Text = $"Sem rota rodoviária entre {origem.Name} e {destino.Name}." +
                (origem.Name == "Macapá" || destino.Name == "Macapá"
                    ? " (Macapá não possui ligação rodoviária com o resto do país.)"
                    : "");
            DrawGraph();
            return;
        }

        _custoNormal = CongestionHelper.CustoDaRota(_grafoNormal, _rotaNormal);
        etapasTitulo.Text = "Etapas — Cenário Normal";
        statusText.Text = $"Rota normal: {RotaTexto(_rotaNormal)} ({_custoNormal:0} km). " +
                          "Use ▶ para ver as etapas.";
        congestionarButton.IsEnabled = cenario2Radio.IsChecked == true;
        nextStepButton.IsEnabled = true;
        runAllButton.IsEnabled = true;
        DrawGraph();
    }

    private void Congestionar_Click(object sender, RoutedEventArgs e)
    {
        if (_rotaNormal is null || _rotaNormal.Count == 0) return;
        if (!double.TryParse(fatorInput.Text, out double fator) || fator <= 1)
        {
            statusText.Text = "Fator inválido — use número > 1 (ex.: 4).";
            return;
        }
        if (!TrySelecionarRota(out var origem, out var destino)) return;

        _trechosCongestionados = CongestionHelper.TrechosDaRota(_rotaNormal);
        _fatorAtual = fator;
        var grafoCongestionado = CongestionHelper.ComCongestionamento(
            _grafoNormal, _trechosCongestionados, fator);

        var astar = new AStar(_heuristica);
        _rotaCongestionada = astar.Find(origem, destino, grafoCongestionado);
        _custoCongestionado = CongestionHelper.CustoDaRota(grafoCongestionado, _rotaCongestionada);
        _steps = astar.Steps;
        _currentStep = 0;
        etapasList.Items.Clear();
        etapasTitulo.Text = $"Etapas — Cenário Congestionado (fator ×{fator:0.#})";

        MostrarComparacao(grafoCongestionado);
        DrawGraph();
    }

    private bool TrySelecionarRota(out Vertex origem, out Vertex destino)
    {
        origem = destino = null!;
        if (origemCombo.SelectedItem is not string nomeOrigem ||
            destinoCombo.SelectedItem is not string nomeDestino)
        {
            statusText.Text = "Selecione origem e destino.";
            return false;
        }
        if (nomeOrigem == nomeDestino)
        {
            statusText.Text = "Origem e destino devem ser diferentes.";
            return false;
        }
        origem = _vertices[nomeOrigem];
        destino = _vertices[nomeDestino];
        return true;
    }

    private void MostrarComparacao(Graph grafoCongestionado)
    {
        if (_rotaNormal is null || _rotaCongestionada is null) return;

        if (_rotaCongestionada.Count == 0)
        {
            comparacaoText.Text = "Congestionamento bloqueou todas as rotas (sem caminho).";
            return;
        }

        // Pesos alterados, trecho a trecho (antes → depois)
        string trechos = string.Join("\n", _trechosCongestionados.Select(t =>
        {
            double antes = CongestionHelper.PesoDoTrecho(_grafoNormal, t.De, t.Para);
            return $"  {t.De.Name}–{t.Para.Name}: {antes:0} → {antes * _fatorAtual:0} km";
        }));

        // Quanto custaria insistir na rota normal com trânsito
        double custoNormalComTransito =
            CongestionHelper.CustoDaRota(grafoCongestionado, _rotaNormal);

        // Primeiro ponto onde as rotas divergem
        int i = 0;
        while (i < _rotaNormal.Count && i < _rotaCongestionada.Count &&
               _rotaNormal[i] == _rotaCongestionada[i])
            i++;
        bool identicas = i >= _rotaNormal.Count && i >= _rotaCongestionada.Count;

        string porque = identicas
            ? $"POR QUÊ MANTEVE: mesmo com trânsito, a rota normal custa " +
              $"{custoNormalComTransito:0} km e qualquer desvio custaria mais — " +
              $"A* fecha sempre o menor F, então enfrenta o congestionamento."
            : $"POR QUÊ DESVIOU: insistir na rota normal custaria {custoNormalComTransito:0} km " +
              $"com trânsito; o desvio custa {_custoCongestionado:0} km " +
              $"({custoNormalComTransito - _custoCongestionado:0} km a menos). " +
              $"A* fecha sempre o menor F, então abandona a rota normal " +
              $"a partir de {_rotaNormal[Math.Max(0, i - 1)].Name}.";

        comparacaoText.Text =
            $"NORMAL ({_custoNormal:0} km): {RotaTexto(_rotaNormal)}\n" +
            $"CONGESTIONADA ({_custoCongestionado:0} km): {RotaTexto(_rotaCongestionada)}\n\n" +
            $"Trechos congestionados (×{_fatorAtual:0.#}):\n{trechos}\n\n" +
            porque;
        statusText.Text = "";
    }

    private static string RotaTexto(List<Vertex> rota) =>
        string.Join(" → ", rota.Select(v => v.Name));

    // ── Etapas (Abertos/Fechados) ────────────────────────────────────────────

    private void NextStep_Click(object sender, RoutedEventArgs e) => ApplyNextStep();

    private void RunAll_Click(object sender, RoutedEventArgs e)
    {
        while (_steps is not null && _currentStep < _steps.Count)
            ApplyNextStep();
    }

    private void ApplyNextStep()
    {
        if (_steps is null || _currentStep >= _steps.Count)
        {
            statusText.Text = "Todas as etapas exibidas.";
            return;
        }

        var step = _steps[_currentStep];
        etapasList.Items.Add($"Etapa {_currentStep + 1}: {step}");
        etapasList.ScrollIntoView(etapasList.Items[^1]);

        // Estado atual das listas (layout do cenário 1 do colega)
        listaAbertaBox.Text = string.Join(", ",
            step.Abertos.Select(i => $"{i.Vertex.Name} (F={i.F:0.#})"));
        listaFechadaBox.Text = string.Join(", ", step.Fechados.Select(v => v.Name));

        _currentStep++;
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ResetResultados();
        statusText.Text = "";
        DrawGraph();
    }

    private void ResetResultados()
    {
        _rotaNormal = null;
        _rotaCongestionada = null;
        _trechosCongestionados = [];
        _steps = null;
        _currentStep = 0;
        etapasList.Items.Clear();
        listaAbertaBox.Text = "";
        listaFechadaBox.Text = "";
        comparacaoText.Text = "";
        etapasTitulo.Text = "Etapas (Abertos / Fechados)";
        congestionarButton.IsEnabled = false;
        nextStepButton.IsEnabled = false;
        runAllButton.IsEnabled = false;
    }

    // ── Desenho do mapa ──────────────────────────────────────────────────────

    private void DrawGraph()
    {
        graphCanvas.Children.Clear();
        if (_vertices is null || graphCanvas.ActualWidth < 50) return;

        var pontos = ProjetarCapitais();

        // Contorno do Brasil (GeoJSON) como referência visual
        if (_contornoBrasil.Count > 0)
        {
            var contorno = new Polygon
            {
                Points = [.. _contornoBrasil.Select(c => Projetar(c.Lon, c.Lat))],
                Fill = new SolidColorBrush(Color.FromRgb(0xF2, 0xF7, 0xF2)),
                Stroke = Brushes.Silver,
                StrokeThickness = 1,
            };
            graphCanvas.Children.Add(contorno);
        }

        // Arestas de fundo opcionais (grafo completo = 351 arestas, vira poluição visual)
        if (mostrarArestasCheck.IsChecked == true)
            foreach (var edge in _grafoNormal.Edges)
                DesenharLinha(pontos, edge.From, edge.To,
                    Brushes.Gainsboro, 0.5, tracejada: false);

        // Rota normal (azul)
        if (_rotaNormal is not null)
            foreach (var (de, para) in CongestionHelper.TrechosDaRota(_rotaNormal))
                DesenharLinha(pontos, de, para, Brushes.RoyalBlue, 3, tracejada: false);

        // Trechos congestionados (tracejado laranja sobre a rota normal)
        foreach (var (de, para) in _trechosCongestionados)
            DesenharLinha(pontos, de, para, Brushes.DarkOrange, 3, tracejada: true);

        // Rota congestionada (vermelho)
        if (_rotaCongestionada is not null)
            foreach (var (de, para) in CongestionHelper.TrechosDaRota(_rotaCongestionada))
                DesenharLinha(pontos, de, para, Brushes.Crimson, 3, tracejada: false);

        // Rótulos de peso dos trechos congestionados — por último, na frente das linhas
        foreach (var (de, para) in _trechosCongestionados)
        {
            double pesoNovo =
                CongestionHelper.PesoDoTrecho(_grafoNormal, de, para) * _fatorAtual;
            var rotulo = new TextBlock
            {
                Text = $"{pesoNovo:0} km",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkOrange,
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)),
                Padding = new Thickness(2, 0, 2, 0),
            };
            rotulo.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Point meio = new(
                (pontos[de].X + pontos[para].X) / 2,
                (pontos[de].Y + pontos[para].Y) / 2);
            Canvas.SetLeft(rotulo, meio.X - rotulo.DesiredSize.Width / 2);
            Canvas.SetTop(rotulo, meio.Y - rotulo.DesiredSize.Height / 2);
            graphCanvas.Children.Add(rotulo);
        }

        // Vértices
        foreach (var (nome, vertex) in _vertices)
        {
            var p = pontos[vertex];
            const double r = 7;
            bool naRotaNormal = _rotaNormal?.Contains(vertex) == true;
            bool naRotaCong = _rotaCongestionada?.Contains(vertex) == true;

            var ellipse = new Ellipse
            {
                Width = r * 2,
                Height = r * 2,
                Fill = naRotaCong ? Brushes.Crimson
                     : naRotaNormal ? Brushes.RoyalBlue
                     : Brushes.LightGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            Canvas.SetLeft(ellipse, p.X - r);
            Canvas.SetTop(ellipse, p.Y - r);
            graphCanvas.Children.Add(ellipse);

            var label = new TextBlock
            {
                Text = nome,
                FontSize = 10,
                FontWeight = naRotaNormal || naRotaCong ? FontWeights.Bold : FontWeights.Normal,
            };

            // Litoral leste: rótulo à esquerda do nó (senão estoura a borda / colide)
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double dy = CapitaisData.AjusteVerticalRotulo.GetValueOrDefault(nome);
            double x = CapitaisData.RotuloEsquerda.Contains(nome)
                ? p.X - r - 2 - label.DesiredSize.Width
                : p.X + r + 2;
            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, p.Y - r + dy);
            graphCanvas.Children.Add(label);
        }
    }

    // Projeta (longitude, latitude) para coordenadas do canvas com escala uniforme
    // (mesmo fator nos dois eixos — preserva o formato do país) e centralizado.
    // Bounding box aproximado do Brasil: lon [-74, -34], lat [-34, 6]
    private Point Projetar(double lon, double lat)
    {
        const double lonMin = -74, lonMax = -34, latMin = -34, latMax = 6;
        const double margem = 30;
        double w = graphCanvas.ActualWidth - 2 * margem;
        double h = graphCanvas.ActualHeight - 2 * margem;

        double escala = Math.Min(w / (lonMax - lonMin), h / (latMax - latMin));
        double offsetX = margem + (w - (lonMax - lonMin) * escala) / 2;
        double offsetY = margem + (h - (latMax - latMin) * escala) / 2;

        double x = offsetX + (lon - lonMin) * escala;
        double y = offsetY + (latMax - lat) * escala;
        return new Point(x, y);
    }

    private Dictionary<Vertex, Point> ProjetarCapitais()
    {
        var pontos = new Dictionary<Vertex, Point>();
        foreach (var vertex in _vertices.Values)
            pontos[vertex] = Projetar(vertex.Position?.X ?? -74, vertex.Position?.Y ?? -34);
        return pontos;
    }

    private void DesenharLinha(
        Dictionary<Vertex, Point> pontos, Vertex de, Vertex para,
        Brush cor, double espessura, bool tracejada)
    {
        var linha = new Line
        {
            X1 = pontos[de].X, Y1 = pontos[de].Y,
            X2 = pontos[para].X, Y2 = pontos[para].Y,
            Stroke = cor,
            StrokeThickness = espessura,
        };
        if (tracejada)
            linha.StrokeDashArray = [4, 3];
        graphCanvas.Children.Add(linha);
    }
}
