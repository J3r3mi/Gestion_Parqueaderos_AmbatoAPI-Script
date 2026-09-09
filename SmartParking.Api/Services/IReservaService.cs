using SmartParking.Api.Dtos.Reservas;

namespace SmartParking.Api.Services;

public class ReservaException : Exception
{
    public ReservaException(string mensaje) : base(mensaje) { }
}

public interface IReservaService
{
    Task<ReservaResponseDto> CrearReservaAsync(int usuarioId, CrearReservaRequestDto request);
    Task CancelarReservaAsync(int usuarioId, int reservaId);
    Task<List<ReservaResponseDto>> MisReservasAsync(int usuarioId);

    Task<ValidarAccesoResponseDto> ValidarEntradaAsync(int operadorId, string qrToken);
    Task<RegistrarSalidaResponseDto> RegistrarSalidaAsync(int operadorId, string qrToken, string metodoPago);
}
