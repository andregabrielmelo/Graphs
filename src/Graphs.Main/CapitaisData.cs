namespace Graphs.Main;

/// <summary>
/// Dados estáticos das capitais: aliases de normalização entre os JSONs
/// e coordenadas geográficas aproximadas (longitude, latitude) para o layout do mapa.
/// </summary>
public static class CapitaisData
{
    // Nomes da tabela IBGE (linha reta) → nomes do distancias.json
    public static readonly Dictionary<string, string> Aliases = new()
    {
        ["São Luís"] = "São Luis",
        ["Aracaju"] = "Aracajú",
        ["Acaraju"] = "Aracajú",
    };

    // (Longitude, Latitude) aproximadas — só para desenho do mapa
    public static readonly Dictionary<string, (double Lon, double Lat)> Coordenadas = new()
    {
        ["Aracajú"] = (-37.07, -10.95),
        ["Belém"] = (-48.49, -1.46),
        ["Belo Horizonte"] = (-43.94, -19.92),
        ["Boa Vista"] = (-60.67, 2.82),
        ["Brasília"] = (-47.88, -15.79),
        ["Campo Grande"] = (-54.65, -20.44),
        ["Cuiabá"] = (-56.10, -15.60),
        ["Curitiba"] = (-49.27, -25.43),
        ["Florianópolis"] = (-48.55, -27.59),
        ["Fortaleza"] = (-38.54, -3.72),
        ["Goiânia"] = (-49.26, -16.69),
        ["João Pessoa"] = (-34.86, -7.12),
        ["Macapá"] = (-51.07, 0.04),
        ["Maceió"] = (-35.74, -9.67),
        ["Manaus"] = (-60.03, -3.10),
        ["Natal"] = (-35.21, -5.79),
        ["Palmas"] = (-48.36, -10.24),
        ["Porto Alegre"] = (-51.23, -30.03),
        ["Porto Velho"] = (-63.90, -8.76),
        ["Recife"] = (-34.88, -8.05),
        ["Rio Branco"] = (-67.81, -9.97),
        ["Rio de Janeiro"] = (-43.17, -22.91),
        ["Salvador"] = (-38.50, -12.97),
        ["São Luis"] = (-44.30, -2.53),
        ["São Paulo"] = (-46.63, -23.55),
        ["Teresina"] = (-42.80, -5.09),
        ["Vitória"] = (-40.34, -20.32),
    };

    // Capitais do litoral leste: rótulo desenhado à esquerda do nó
    // (à direita estoura a borda do canvas ou colide com vizinhas)
    public static readonly HashSet<string> RotuloEsquerda =
    [
        "João Pessoa", "Recife", "Natal", "Maceió", "Aracajú",
        "Salvador", "Vitória", "Rio de Janeiro", "São Paulo", "São Luis",
    ];

    // Deslocamento vertical (px) para separar pares de rótulos muito próximos
    public static readonly Dictionary<string, double> AjusteVerticalRotulo = new()
    {
        ["João Pessoa"] = -6,
        ["Recife"] = 6,
        ["Natal"] = -4,
        ["Maceió"] = -2,
        ["Aracajú"] = 6,
        ["Vitória"] = 6,
        ["Belo Horizonte"] = -4,
        ["Rio de Janeiro"] = 6,
        ["São Paulo"] = -4,
        ["Goiânia"] = 6,
        ["Brasília"] = -4,
    };
}
