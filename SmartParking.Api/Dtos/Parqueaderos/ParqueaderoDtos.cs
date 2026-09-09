using System.ComponentModel.DataAnnotations;
using SmartParking.Api.Models;

namespace SmartParking.Api.Dtos.Parqueaderos;

// ---------- Lectura (para el mapa y el flujo de reserva del conductor) ----------

public class ParqueaderoListItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public int CapacidadTotal { get; set; }
    public int CuposLibres { get; set; }
}

public class PlazaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // libre | reservada | ocupada | mantenimiento
    public string TipoVehiculo { get; set; } = string.Empty;
}

public class TarifaDto
{
    public int Id { get; set; }
    public string TipoVehiculo { get; set; } = string.Empty;
    public decimal ValorHora { get; set; }
    public DateTime VigenteDesde { get; set; }
}

// ---------- Escritura (panel de Administrador) ----------

public class CrearParqueaderoRequestDto
{
    [Required] public string Nombre { get; set; } = string.Empty;
    [Required] public string Direccion { get; set; } = string.Empty;

    [Range(-5, 2, ErrorMessage = "Latitud fuera de rango para Ecuador")]
    public decimal Latitud { get; set; }

    [Range(-92, -75, ErrorMessage = "Longitud fuera de rango para Ecuador")]
    public decimal Longitud { get; set; }

    [Range(1, 1000)]
    public int CapacidadTotal { get; set; }
}

public class ActualizarParqueaderoRequestDto
{
    [Required] public string Nombre { get; set; } = string.Empty;
    [Required] public string Direccion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public bool Activo { get; set; } = true;
}

public class CrearPlazaRequestDto
{
    [Required, MaxLength(10)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public TipoVehiculo TipoVehiculo { get; set; } = TipoVehiculo.auto;
}

public class ActualizarEstadoPlazaRequestDto
{
    [Required]
    public EstadoPlaza Estado { get; set; }
}

public class CrearTarifaRequestDto
{
    [Required]
    public TipoVehiculo TipoVehiculo { get; set; } = TipoVehiculo.auto;

    [Range(0.01, 100)]
    public decimal ValorHora { get; set; }
}
