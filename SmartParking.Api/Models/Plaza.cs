namespace SmartParking.Api.Models;

public class Plaza
{
    public int Id { get; set; }
    public int ParqueaderoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public EstadoPlaza Estado { get; set; } = EstadoPlaza.libre;
    public TipoVehiculo TipoVehiculo { get; set; } = TipoVehiculo.auto;
    public int Version { get; set; } // reservado para futuro locking optimista
    public DateTime CreatedAt { get; set; }

    public Parqueadero? Parqueadero { get; set; }
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
