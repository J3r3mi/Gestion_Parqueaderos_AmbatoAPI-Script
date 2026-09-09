namespace SmartParking.Api.Models;

public class Tarifa
{
    public int Id { get; set; }
    public int ParqueaderoId { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; } = TipoVehiculo.auto;
    public decimal ValorHora { get; set; }
    public DateTime VigenteDesde { get; set; }

    public Parqueadero? Parqueadero { get; set; }
}
