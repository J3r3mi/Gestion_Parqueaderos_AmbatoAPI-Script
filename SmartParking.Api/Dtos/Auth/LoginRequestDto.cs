using System.ComponentModel.DataAnnotations;

namespace SmartParking.Api.Dtos.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El correo o cédula es obligatorio")]
    public string Identificador { get; set; } = string.Empty; // correo o cédula

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}
