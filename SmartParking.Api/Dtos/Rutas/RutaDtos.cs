namespace SmartParking.Api.Dtos.Rutas;

public class PuntoRutaDto
{
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string NombreNodo { get; set; } = string.Empty;
}

public class TramoRutaDto
{
    public string NombreCalle { get; set; } = string.Empty;
    public double DistanciaMetros { get; set; }
    public bool EsCalleSaturada { get; set; }
}

public class RutaResponseDto
{
    public string Algoritmo { get; set; } = string.Empty; // "dijkstra" | "astar"
    public List<PuntoRutaDto> Puntos { get; set; } = new();
    public List<TramoRutaDto> Tramos { get; set; } = new();
    public double DistanciaTotalMetros { get; set; }
    public double TiempoEstimadoMinutos { get; set; }
    public int NodosExplorados { get; set; } // para comparar eficiencia Dijkstra vs A* en la defensa
}
