using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class Parqueadero
    {
        [Key]
        public int IdParqueadero { get; set; }

        public int IdAdministrador { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        public string? Telefono { get; set; }

        public string? HorarioAtencion { get; set; }

        public bool Estado { get; set; }

        // Relación con el administrador
        public Usuario? Administrador { get; set; }

        // Relación con los espacios
        public ICollection<Espacio> Espacios { get; set; } = new List<Espacio>();
    }
}
