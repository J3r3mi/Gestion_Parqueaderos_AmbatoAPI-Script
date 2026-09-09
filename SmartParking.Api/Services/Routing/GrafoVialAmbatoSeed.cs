using SmartParking.Api.Models.Vial;

namespace SmartParking.Api.Services.Routing;

/// <summary>
/// Grafo vial SIMPLIFICADO del centro de Ambato.
///
/// ADVERTENCIA IMPORTANTE (documentar en la tesis, no ocultar):
/// Las coordenadas de los nodos son APROXIMADAS. Se construyeron ubicando manualmente,
/// sobre un mapa, los cruces conocidos de Cevallos, Bolívar y Sucre con las calles
/// transversales Martínez, Quito, Guayaquil y Rocafuerte — no provienen de un dataset
/// abierto de intersecciones geoespaciales verificado (no existe uno disponible
/// públicamente con esa granularidad para Ambato al momento de construir esto).
///
/// Para producción real, reemplaza este seed por un extracto real de OpenStreetMap:
///   1. Overpass API, query tipo:
///        [out:json];
///        way["highway"]["name"~"Cevallos|Bolívar|Sucre|Martínez|Quito|Guayaquil|Rocafuerte"]
///          (around:1500,-1.24177,-78.62279);
///        (._;>;);
///        out body;
///   2. Convertir cada `way` en una secuencia de NodoVial/AristaVial (cada segmento entre
///      dos nodos consecutivos de la way es una arista).
///   3. Marcar EsCalleSaturada según el campo `name` de la way.
///
/// Mientras tanto, este grafo es suficiente para demostrar que el algoritmo de
/// optimización de rutas (Dijkstra/A*) funciona correctamente end-to-end.
/// </summary>
public static class GrafoVialAmbatoSeed
{
    public static GrafoVial Obtener()
    {
        var grafo = new GrafoVial();

        // ---------------- NODOS (intersecciones aproximadas) ----------------
        grafo.Nodos.AddRange(new[]
        {
            new NodoVial { Id = "CEV_MART",  Nombre = "Cevallos y Martínez",   Latitud = -1.243100, Longitud = -78.622600 },
            new NodoVial { Id = "CEV_QUI",   Nombre = "Cevallos y Quito",      Latitud = -1.241900, Longitud = -78.622900 },
            new NodoVial { Id = "CEV_GYE",   Nombre = "Cevallos y Guayaquil",  Latitud = -1.240700, Longitud = -78.623200 },
            new NodoVial { Id = "CEV_ROCA",  Nombre = "Cevallos y Rocafuerte", Latitud = -1.239600, Longitud = -78.623500 },

            new NodoVial { Id = "BOL_MART",  Nombre = "Bolívar y Martínez",    Latitud = -1.243300, Longitud = -78.621400 },
            new NodoVial { Id = "BOL_QUI",   Nombre = "Bolívar y Quito",       Latitud = -1.242100, Longitud = -78.621700 },
            new NodoVial { Id = "BOL_GYE",   Nombre = "Bolívar y Guayaquil",   Latitud = -1.240900, Longitud = -78.622000 },
            new NodoVial { Id = "BOL_ROCA",  Nombre = "Bolívar y Rocafuerte",  Latitud = -1.239800, Longitud = -78.622300 },

            new NodoVial { Id = "SUC_MART",  Nombre = "Sucre y Martínez",      Latitud = -1.243500, Longitud = -78.620200 },
            new NodoVial { Id = "SUC_QUI",   Nombre = "Sucre y Quito",         Latitud = -1.242300, Longitud = -78.620500 },
            new NodoVial { Id = "SUC_GYE",   Nombre = "Sucre y Guayaquil",     Latitud = -1.241100, Longitud = -78.620800 },
            new NodoVial { Id = "SUC_ROCA",  Nombre = "Sucre y Rocafuerte",    Latitud = -1.240000, Longitud = -78.621100 },

            // Ruta alterna por fuera del centro (menos directa pero sin las calles saturadas)
            new NodoVial { Id = "ALT_NORTE", Nombre = "Circunvalación Norte",  Latitud = -1.238800, Longitud = -78.624800 },
            new NodoVial { Id = "ALT_SUR",   Nombre = "Circunvalación Sur",    Latitud = -1.244600, Longitud = -78.619000 },

            // Parqueadero demo del schema.sql (Cevallos y Bolívar, centro)
            new NodoVial { Id = "PARQ_1",    Nombre = "Parqueadero Centro Ambato", Latitud = -1.24177, Longitud = -78.62279 }
        });

        // ---------------- ARISTAS ----------------
        // Tramos verticales de Cevallos, Bolívar y Sucre: marcados como saturados.
        AgregarTramoSaturado(grafo, "CEV_MART", "CEV_QUI", "Av. Cevallos", 135);
        AgregarTramoSaturado(grafo, "CEV_QUI", "CEV_GYE", "Av. Cevallos", 135);
        AgregarTramoSaturado(grafo, "CEV_GYE", "CEV_ROCA", "Av. Cevallos", 125);

        AgregarTramoSaturado(grafo, "BOL_MART", "BOL_QUI", "Calle Bolívar", 135);
        AgregarTramoSaturado(grafo, "BOL_QUI", "BOL_GYE", "Calle Bolívar", 135);
        AgregarTramoSaturado(grafo, "BOL_GYE", "BOL_ROCA", "Calle Bolívar", 125);

        AgregarTramoSaturado(grafo, "SUC_MART", "SUC_QUI", "Calle Sucre", 135);
        AgregarTramoSaturado(grafo, "SUC_QUI", "SUC_GYE", "Calle Sucre", 135);
        AgregarTramoSaturado(grafo, "SUC_GYE", "SUC_ROCA", "Calle Sucre", 125);

        // Tramos transversales: no saturados, conectan las tres avenidas principales.
        AgregarTramoLibre(grafo, "CEV_MART", "BOL_MART", "Calle Martínez", 120);
        AgregarTramoLibre(grafo, "BOL_MART", "SUC_MART", "Calle Martínez", 120);
        AgregarTramoLibre(grafo, "CEV_QUI", "BOL_QUI", "Calle Quito", 120);
        AgregarTramoLibre(grafo, "BOL_QUI", "SUC_QUI", "Calle Quito", 120);
        AgregarTramoLibre(grafo, "CEV_GYE", "BOL_GYE", "Calle Guayaquil", 120);
        AgregarTramoLibre(grafo, "BOL_GYE", "SUC_GYE", "Calle Guayaquil", 120);
        AgregarTramoLibre(grafo, "CEV_ROCA", "BOL_ROCA", "Calle Rocafuerte", 120);
        AgregarTramoLibre(grafo, "BOL_ROCA", "SUC_ROCA", "Calle Rocafuerte", 120);

        // Ruta alterna periférica: más larga en distancia, pero libre de congestión.
        AgregarTramoLibre(grafo, "CEV_ROCA", "ALT_NORTE", "Circunvalación Norte", 480);
        AgregarTramoLibre(grafo, "ALT_NORTE", "SUC_ROCA", "Circunvalación Norte", 620);
        AgregarTramoLibre(grafo, "CEV_MART", "ALT_SUR", "Circunvalación Sur", 500);
        AgregarTramoLibre(grafo, "ALT_SUR", "SUC_MART", "Circunvalación Sur", 640);

        // El parqueadero demo está justo en Cevallos y Bolívar (entre CEV_QUI y BOL_QUI).
        AgregarTramoLibre(grafo, "CEV_QUI", "PARQ_1", "Acceso parqueadero", 40);
        AgregarTramoLibre(grafo, "BOL_QUI", "PARQ_1", "Acceso parqueadero", 40);

        return grafo;
    }

    private static void AgregarTramoSaturado(GrafoVial grafo, string a, string b, string calle, double metros)
        => AgregarTramo(grafo, a, b, calle, metros, saturada: true);

    private static void AgregarTramoLibre(GrafoVial grafo, string a, string b, string calle, double metros)
        => AgregarTramo(grafo, a, b, calle, metros, saturada: false);

    private static void AgregarTramo(GrafoVial grafo, string a, string b, string calle, double metros, bool saturada)
    {
        grafo.Aristas.Add(new AristaVial
        {
            NodoOrigenId = a, NodoDestinoId = b, NombreCalle = calle,
            DistanciaMetros = metros, EsCalleSaturada = saturada
        });
        // Grafo no dirigido: agregamos también la arista inversa.
        grafo.Aristas.Add(new AristaVial
        {
            NodoOrigenId = b, NodoDestinoId = a, NombreCalle = calle,
            DistanciaMetros = metros, EsCalleSaturada = saturada
        });
    }
}
