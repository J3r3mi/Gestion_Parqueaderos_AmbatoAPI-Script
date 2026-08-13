namespace GestionParqueaderosAmbato.API.DTOs
{
    public class LoginRespuestaDto
    {
        public string Token { get; set; } = string.Empty;

        public UsuarioDto Usuario { get; set; } = new UsuarioDto();
    }
}