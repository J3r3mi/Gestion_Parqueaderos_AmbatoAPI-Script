using System.ComponentModel.DataAnnotations;

namespace SmartParking.Api.Dtos.Usuarios;

public class UsuarioListItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ActualizarEstadoUsuarioRequestDto
{
    [Required]
    public bool Activo { get; set; }
}
