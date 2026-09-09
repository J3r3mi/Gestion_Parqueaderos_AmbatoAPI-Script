using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Rutas;
using SmartParking.Api.Models.Vial;
using Microsoft.EntityFrameworkCore;

namespace SmartParking.Api.Services.Routing;

public class RutaException : Exception
{
    public RutaException(string mensaje) : base(mensaje) { }
}

public interface IRoutingService
{
    Task<RutaResponseDto> CalcularRutaAsync(double origenLat, double origenLng, int parqueaderoId, string algoritmo);
}

public class RoutingService : IRoutingService
{
    private readonly AppDbContext _db;

    private const double VelocidadPromedioKmH = 25.0; // velocidad urbana promedio asumida
    private const int VecinosParaConectarPunto = 2;   // a cuántos nodos reales se conecta un punto arbitrario (GPS/parqueadero)

    public RoutingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RutaResponseDto> CalcularRutaAsync(
        double origenLat, double origenLng, int parqueaderoId, string algoritmo)
    {
        var parqueadero = await _db.Parqueaderos.FindAsync(parqueaderoId)
            ?? throw new RutaException("Parqueadero no encontrado.");

        var distCentroMetros = DistanciaHaversineMetros(origenLat, origenLng, -1.24177, -78.62279);
        if (distCentroMetros > 20000)
        {
            origenLat = -1.2460;
            origenLng = -78.6240;
        }

        var grafo = GrafoVialAmbatoSeed.Obtener();

        const string idOrigen = "__ORIGEN__";
        const string idDestino = "__DESTINO__";

        grafo.Nodos.Add(new NodoVial { Id = idOrigen, Nombre = "Tu ubicación", Latitud = origenLat, Longitud = origenLng });
        grafo.Nodos.Add(new NodoVial { Id = idDestino, Nombre = parqueadero.Nombre, Latitud = (double)parqueadero.Latitud, Longitud = (double)parqueadero.Longitud });

        ConectarPuntoAlGrafoMasCercano(grafo, idOrigen);
        ConectarPuntoAlGrafoMasCercano(grafo, idDestino);

        var adyacencia = ConstruirListaAdyacencia(grafo);
        var horaActual = DateTime.Now; // hora local del servidor, usada para simular congestión por franja horaria

        var (camino, tiempoTotalMin, nodosExplorados) = algoritmo.ToLower() == "dijkstra"
            ? EjecutarDijkstra(adyacencia, idOrigen, idDestino, horaActual)
            : EjecutarAEstrella(adyacencia, grafo, idOrigen, idDestino, horaActual);

        if (camino.Count == 0)
            throw new RutaException("No se encontró una ruta entre el origen y el parqueadero.");

        return ConstruirRespuesta(grafo, adyacencia, camino, algoritmo, tiempoTotalMin, nodosExplorados);
    }

    // -----------------------------------------------------------------
    // CONEXIÓN DE PUNTOS ARBITRARIOS (GPS del conductor, coordenadas del parqueadero)
    // -----------------------------------------------------------------
    private void ConectarPuntoAlGrafoMasCercano(GrafoVial grafo, string idPunto)
    {
        var punto = grafo.Nodos.First(n => n.Id == idPunto);

        var vecinosCercanos = grafo.Nodos
            .Where(n => n.Id != idPunto)
            .OrderBy(n => DistanciaHaversineMetros(punto.Latitud, punto.Longitud, n.Latitud, n.Longitud))
            .Take(VecinosParaConectarPunto)
            .ToList();

        foreach (var vecino in vecinosCercanos)
        {
            var distancia = DistanciaHaversineMetros(punto.Latitud, punto.Longitud, vecino.Latitud, vecino.Longitud);
            grafo.Aristas.Add(new AristaVial
            {
                NodoOrigenId = idPunto, NodoDestinoId = vecino.Id,
                NombreCalle = "Acceso", DistanciaMetros = distancia, EsCalleSaturada = false
            });
            grafo.Aristas.Add(new AristaVial
            {
                NodoOrigenId = vecino.Id, NodoDestinoId = idPunto,
                NombreCalle = "Acceso", DistanciaMetros = distancia, EsCalleSaturada = false
            });
        }
    }

    private static Dictionary<string, List<AristaVial>> ConstruirListaAdyacencia(GrafoVial grafo)
    {
        var adyacencia = grafo.Nodos.ToDictionary(n => n.Id, _ => new List<AristaVial>());
        foreach (var arista in grafo.Aristas)
            adyacencia[arista.NodoOrigenId].Add(arista);
        return adyacencia;
    }

    // -----------------------------------------------------------------
    // PESO DE UNA ARISTA: tiempo estimado en minutos, penalizado si está saturada en hora pico
    // -----------------------------------------------------------------
    private static double TiempoMinutos(AristaVial arista, DateTime hora)
    {
        var tiempoBaseMin = (arista.DistanciaMetros / 1000.0) / VelocidadPromedioKmH * 60.0;
        return tiempoBaseMin * FactorCongestion(arista.EsCalleSaturada, hora);
    }

    private static double FactorCongestion(bool esCalleSaturada, DateTime hora)
    {
        if (!esCalleSaturada) return 1.0;

        bool esHoraPico =
            (hora.Hour >= 7 && hora.Hour < 9) ||
            (hora.Hour >= 12 && hora.Hour < 14) ||
            (hora.Hour >= 17 && hora.Hour < 20);

        return esHoraPico ? 2.5 : 1.3; // fuera de pico, sigue algo más lenta que una calle libre
    }

    // -----------------------------------------------------------------
    // DIJKSTRA
    // -----------------------------------------------------------------
    private static (List<string> camino, double tiempoMin, int nodosExplorados) EjecutarDijkstra(
        Dictionary<string, List<AristaVial>> adyacencia, string origen, string destino, DateTime hora)
    {
        var distancias = adyacencia.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var anteriores = new Dictionary<string, string?>();
        var visitados = new HashSet<string>();
        var cola = new PriorityQueue<string, double>();

        distancias[origen] = 0;
        cola.Enqueue(origen, 0);

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            if (!visitados.Add(actual)) continue; // ya procesado con un costo menor
            if (actual == destino) break;

            foreach (var arista in adyacencia[actual])
            {
                var nuevoCosto = distancias[actual] + TiempoMinutos(arista, hora);
                if (nuevoCosto < distancias[arista.NodoDestinoId])
                {
                    distancias[arista.NodoDestinoId] = nuevoCosto;
                    anteriores[arista.NodoDestinoId] = actual;
                    cola.Enqueue(arista.NodoDestinoId, nuevoCosto);
                }
            }
        }

        var camino = ReconstruirCamino(anteriores, origen, destino);
        return (camino, camino.Count > 0 ? distancias[destino] : 0, visitados.Count);
    }

    // -----------------------------------------------------------------
    // A* — misma idea que Dijkstra, pero prioriza con heurística (distancia en línea recta al destino)
    // -----------------------------------------------------------------
    private static (List<string> camino, double tiempoMin, int nodosExplorados) EjecutarAEstrella(
        Dictionary<string, List<AristaVial>> adyacencia, GrafoVial grafo, string origen, string destino, DateTime hora)
    {
        var nodosPorId = grafo.Nodos.ToDictionary(n => n.Id);
        var nodoDestino = nodosPorId[destino];

        double Heuristica(string nodoId)
        {
            var n = nodosPorId[nodoId];
            var distanciaLineaRecta = DistanciaHaversineMetros(n.Latitud, n.Longitud, nodoDestino.Latitud, nodoDestino.Longitud);
            // Heurística admisible: asume la velocidad más favorable posible (sin congestión),
            // por eso nunca sobreestima el costo real restante.
            return (distanciaLineaRecta / 1000.0) / VelocidadPromedioKmH * 60.0;
        }

        var costoReal = adyacencia.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var anteriores = new Dictionary<string, string?>();
        var visitados = new HashSet<string>();
        var cola = new PriorityQueue<string, double>();

        costoReal[origen] = 0;
        cola.Enqueue(origen, Heuristica(origen));

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            if (!visitados.Add(actual)) continue;
            if (actual == destino) break;

            foreach (var arista in adyacencia[actual])
            {
                var nuevoCosto = costoReal[actual] + TiempoMinutos(arista, hora);
                if (nuevoCosto < costoReal[arista.NodoDestinoId])
                {
                    costoReal[arista.NodoDestinoId] = nuevoCosto;
                    anteriores[arista.NodoDestinoId] = actual;
                    cola.Enqueue(arista.NodoDestinoId, nuevoCosto + Heuristica(arista.NodoDestinoId));
                }
            }
        }

        var camino = ReconstruirCamino(anteriores, origen, destino);
        return (camino, camino.Count > 0 ? costoReal[destino] : 0, visitados.Count);
    }

    private static List<string> ReconstruirCamino(Dictionary<string, string?> anteriores, string origen, string destino)
    {
        if (origen == destino) return new List<string> { origen };
        if (!anteriores.ContainsKey(destino)) return new List<string>(); // sin ruta posible

        var camino = new List<string> { destino };
        var actual = destino;
        while (actual != origen)
        {
            actual = anteriores[actual]!;
            camino.Add(actual);
        }
        camino.Reverse();
        return camino;
    }

    // -----------------------------------------------------------------
    private static RutaResponseDto ConstruirRespuesta(
        GrafoVial grafo, Dictionary<string, List<AristaVial>> adyacencia,
        List<string> camino, string algoritmo, double tiempoMin, int nodosExplorados)
    {
        var nodosPorId = grafo.Nodos.ToDictionary(n => n.Id);
        var respuesta = new RutaResponseDto
        {
            Algoritmo = algoritmo.ToLower(),
            TiempoEstimadoMinutos = Math.Round(tiempoMin, 1),
            NodosExplorados = nodosExplorados
        };

        double distanciaTotal = 0;

        for (int i = 0; i < camino.Count; i++)
        {
            var nodo = nodosPorId[camino[i]];
            respuesta.Puntos.Add(new PuntoRutaDto
            {
                Latitud = nodo.Latitud, Longitud = nodo.Longitud, NombreNodo = nodo.Nombre
            });

            if (i < camino.Count - 1)
            {
                var arista = adyacencia[camino[i]].First(a => a.NodoDestinoId == camino[i + 1]);
                distanciaTotal += arista.DistanciaMetros;
                respuesta.Tramos.Add(new TramoRutaDto
                {
                    NombreCalle = arista.NombreCalle,
                    DistanciaMetros = Math.Round(arista.DistanciaMetros, 0),
                    EsCalleSaturada = arista.EsCalleSaturada
                });
            }
        }

        respuesta.DistanciaTotalMetros = Math.Round(distanciaTotal, 0);
        return respuesta;
    }

    // -----------------------------------------------------------------
    // Distancia en línea recta entre dos coordenadas GPS (fórmula de Haversine)
    // -----------------------------------------------------------------
    private static double DistanciaHaversineMetros(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierraMetros = 6371000;
        double ToRad(double grados) => grados * Math.PI / 180.0;

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return radioTierraMetros * c;
    }
}
