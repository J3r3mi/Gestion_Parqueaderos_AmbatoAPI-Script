using SmartParking.Api.Dtos.Auth;

namespace SmartParking.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<(bool exito, string? error)> RegistrarConductorAsync(RegisterRequestDto request);
    Task<(bool exito, string? error)> RegistrarStaffAsync(RegistrarStaffRequestDto request);
    Task<SolicitarRecuperacionResponseDto> SolicitarRecuperacionAsync(SolicitarRecuperacionRequestDto request);
    Task<(bool exito, string? error)> RestablecerPasswordAsync(RestablecerPasswordRequestDto request);
}
