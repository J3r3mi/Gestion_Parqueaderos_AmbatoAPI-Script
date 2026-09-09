using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public interface IJwtService
{
    (string token, DateTime expiraEn) GenerarToken(Usuario usuario);
}
