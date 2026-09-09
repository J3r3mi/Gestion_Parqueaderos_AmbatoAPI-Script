namespace SmartParking.Api.Models;

public class TransaccionPago
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public decimal Monto { get; set; }
    public MetodoPago MetodoPago { get; set; } = MetodoPago.efectivo;
    public EstadoPago Estado { get; set; } = EstadoPago.pendiente;
    public DateTime FechaHora { get; set; }

    public Reserva? Reserva { get; set; }
}
