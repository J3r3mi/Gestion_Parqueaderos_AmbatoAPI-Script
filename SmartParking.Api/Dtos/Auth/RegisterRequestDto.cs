using System.ComponentModel.DataAnnotations;

namespace SmartParking.Api.Dtos.Auth;

public class RegisterRequestDto
{
    [Required] public string Nombre { get; set; } = string.Empty;
    [Required] public string Cedula { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}
