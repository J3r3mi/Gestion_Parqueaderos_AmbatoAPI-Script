namespace SmartParking.Api.Models;

public class Parqueadero
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public int CapacidadTotal { get; set; }
    public int? AdministradorId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Usuario? Administrador { get; set; }
    public ICollection<Plaza> Plazas { get; set; } = new List<Plaza>();
    public ICollection<Tarifa> Tarifas { get; set; } = new List<Tarifa>();
}
