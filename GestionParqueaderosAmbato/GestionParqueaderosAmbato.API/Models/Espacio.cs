using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class Espacio
    {
        [Key]
        public int IdEspacio { get; set; }

        public int IdParqueadero { get; set; }

        public string NumeroEspacio { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string? Observacion { get; set; }

        // Relación con Parqueadero
        public Parqueadero? Parqueadero { get; set; }

        // Relación con Reservas
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
