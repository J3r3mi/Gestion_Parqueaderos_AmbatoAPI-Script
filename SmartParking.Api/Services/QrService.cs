using System.Security.Cryptography;

namespace SmartParking.Api.Services;

public interface IQrService
{
    string GenerarTokenSeguro();
}

public class QrService : IQrService
{
    /// <summary>
    /// Genera un token aleatorio criptográficamente seguro (256 bits), codificado en
    /// Base64Url para que sea seguro dentro de una URL o un QR sin caracteres especiales.
    /// A diferencia del mockup, esto NO es un JWT: es un identificador opaco de un solo
    /// uso que solo tiene sentido si existe en la tabla `reservas` del servidor.
    /// </summary>
    public string GenerarTokenSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
