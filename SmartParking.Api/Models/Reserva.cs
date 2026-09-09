namespace SmartParking.Api.Models;

public class Reserva
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int PlazaId { get; set; }
    public DateTime HoraReserva { get; set; }
    public DateTime HoraEstimadaArribo { get; set; }
    public DateTime? HoraLlegadaReal { get; set; }
    public DateTime? HoraSalida { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.pendiente;
    public string? QrToken { get; set; }
    public DateTime? QrExpiraEn { get; set; }
    public DateTime CreatedAt { get; set; }

    public Usuario? Usuario { get; set; }
    public Plaza? Plaza { get; set; }
    public ICollection<Acceso> Accesos { get; set; } = new List<Acceso>();
    public ICollection<TransaccionPago> Pagos { get; set; } = new List<TransaccionPago>();
}
