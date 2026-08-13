using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Relación con Rol
        public Rol? Rol { get; set; }

        // Relaciones
        public ICollection<Parqueadero> Parqueaderos { get; set; } = new List<Parqueadero>();

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
