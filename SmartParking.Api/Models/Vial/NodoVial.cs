namespace SmartParking.Api.Models.Vial;

/// <summary>Un nodo representa una intersección o punto relevante de la red vial.</summary>
public class NodoVial
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty; // ej. "Cevallos y Bolívar"
    public double Latitud { get; set; }
    public double Longitud { get; set; }
}

/// <summary>
/// Una arista representa un tramo de calle entre dos nodos. El grafo es no dirigido
/// (los tramos se recorren en ambos sentidos) porque no modelamos sentidos únicos todavía.
/// </summary>
public class AristaVial
{
    public string NodoOrigenId { get; set; } = string.Empty;
    public string NodoDestinoId { get; set; } = string.Empty;
    public string NombreCalle { get; set; } = string.Empty;
    public double DistanciaMetros { get; set; }

    /// <summary>
    /// true para tramos de Cevallos, Bolívar y Sucre en el centro — las calles que,
    /// según el planteamiento del proyecto, se busca evitar en horas pico.
    /// </summary>
    public bool EsCalleSaturada { get; set; }
}

public class GrafoVial
{
    public List<NodoVial> Nodos { get; set; } = new();
    public List<AristaVial> Aristas { get; set; } = new();
}
