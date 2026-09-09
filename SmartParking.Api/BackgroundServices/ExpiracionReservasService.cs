using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Models;

namespace SmartParking.Api.BackgroundServices;

/// <summary>
/// Corre cada minuto. Busca reservas 'pendiente' cuyo qr_expira_en ya pasó
/// (el conductor nunca llegó a validar el QR en puerta) y libera la plaza.
/// Esto es lo que alimenta la alerta de "reservas vencidas no reclamadas" del dashboard.
/// </summary>
public class ExpiracionReservasService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiracionReservasService> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);

    public ExpiracionReservasService(IServiceProvider serviceProvider, ILogger<ExpiracionReservasService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirarReservasVencidasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al expirar reservas vencidas.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task ExpirarReservasVencidasAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Misma disciplina transaccional que en ReservaService: cada reserva vencida
        // se procesa en su propia transacción para no bloquear el resto si una falla.
        var vencidas = await db.Reservas
            .Where(r => r.Estado == EstadoReserva.pendiente && r.QrExpiraEn != null && r.QrExpiraEn < DateTime.UtcNow)
            .Include(r => r.Plaza)
            .ToListAsync(ct);

        if (vencidas.Count == 0)
            return;

        foreach (var reserva in vencidas)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                reserva.Estado = EstadoReserva.expirada;
                if (reserva.Plaza is { Estado: EstadoPlaza.reservada })
                    reserva.Plaza.Estado = EstadoPlaza.libre;

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation("Reserva {ReservaId} expirada automáticamente, plaza liberada.", reserva.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "No se pudo expirar la reserva {ReservaId}.", reserva.Id);
            }
        }
    }
}
