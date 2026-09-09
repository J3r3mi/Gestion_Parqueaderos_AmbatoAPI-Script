using System.ComponentModel.DataAnnotations;

namespace SmartParking.Api.Dtos.Auth;

public class SolicitarRecuperacionRequestDto
{
    [Required] public string Identificador { get; set; } = string.Empty; // correo o cédula
}

public class SolicitarRecuperacionResponseDto
{
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// SOLO PARA DEMO — este entorno no tiene un servicio de correo real configurado.
    /// En producción, este token NUNCA debe devolverse en la respuesta de la API:
    /// debe enviarse únicamente al correo del usuario (SendGrid, AWS SES, SMTP, etc.).
    /// Se expone aquí solo para poder probar el flujo completo sin infraestructura de correo.
    /// </summary>
    public string? TokenSoloParaDemo { get; set; }
}

public class RestablecerPasswordRequestDto
{
    [Required] public string Token { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NuevaPassword { get; set; } = string.Empty;
}
