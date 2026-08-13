using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }

        public int IdUsuario { get; set; }

        public int IdEspacio { get; set; }

        public DateTime FechaReserva { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        // Relaciones
        public Usuario? Usuario { get; set; }

        public Espacio? Espacio { get; set; }

        public ICollection<HistorialReserva> Historial { get; set; } = new List<HistorialReserva>();
    }
}
