namespace SmartParking.Api.Models;

public class Acceso
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public int? OperadorId { get; set; }
    public TipoAcceso Tipo { get; set; }
    public DateTime FechaHora { get; set; }

    public Reserva? Reserva { get; set; }
    public Usuario? Operador { get; set; }
}
