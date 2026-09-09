namespace SmartParking.Api.Models;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public bool Usado { get; set; }
    public DateTime CreatedAt { get; set; }

    public Usuario? Usuario { get; set; }
}
