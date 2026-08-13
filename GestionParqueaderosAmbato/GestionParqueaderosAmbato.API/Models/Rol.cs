using System.ComponentModel.DataAnnotations;

namespace GestionParqueaderosAmbato.API.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
