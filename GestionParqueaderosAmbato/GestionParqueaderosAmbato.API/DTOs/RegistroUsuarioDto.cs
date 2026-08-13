namespace GestionParqueaderosAmbato.API.DTOs
{
    public class RegistroUsuarioDto
    {
        public int IdRol { get; set; }

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
