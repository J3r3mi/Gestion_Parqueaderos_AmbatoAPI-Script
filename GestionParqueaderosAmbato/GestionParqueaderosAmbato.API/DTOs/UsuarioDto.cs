namespace GestionParqueaderosAmbato.API.DTOs
{
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
