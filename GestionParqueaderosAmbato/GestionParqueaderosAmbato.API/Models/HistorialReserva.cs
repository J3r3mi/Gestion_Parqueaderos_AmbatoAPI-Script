using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class HistorialReserva
    {
        [Key]
        public int IdHistorial { get; set; }

        public int IdReserva { get; set; }

        public string? EstadoAnterior { get; set; }

        public string EstadoNuevo { get; set; } = string.Empty;

        public DateTime FechaCambio { get; set; }

        public string? Observacion { get; set; }

        // Relación con Reserva
        public Reserva? Reserva { get; set; }
    }
}
