using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Reservas;
using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public class ReservaService : IReservaService
{
    private readonly AppDbContext _db;
    private readonly IQrService _qr;

    // Ventana de validez del QR y margen de tolerancia antes de expirar una reserva no reclamada.
    private static readonly TimeSpan ToleranciaLlegada = TimeSpan.FromMinutes(20);

    public ReservaService(AppDbContext db, IQrService qr)
    {
        _db = db;
        _qr = qr;
    }

    // -----------------------------------------------------------------
    // CREAR RESERVA - aquí vive el locking pesimista (SELECT ... FOR UPDATE)
    // -----------------------------------------------------------------
    public async Task<ReservaResponseDto> CrearReservaAsync(int usuarioId, CrearReservaRequestDto request)
    {
        // BeginTransactionAsync + FOR UPDATE: mientras esta transacción esté abierta,
        // cualquier otra petición que intente leer esta misma fila con FOR UPDATE
        // queda esperando hasta que hagamos COMMIT o ROLLBACK. Así se evita que dos
        // conductores reserven la misma plaza en el mismo instante.
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var plaza = await _db.Plazas
                .FromSqlInterpolated($"SELECT * FROM plazas WHERE id = {request.PlazaId} FOR UPDATE")
                .Include(p => p.Parqueadero)
                .FirstOrDefaultAsync();

            if (plaza is null)
                throw new ReservaException("La plaza indicada no existe.");

            if (plaza.Estado != EstadoPlaza.libre)
                throw new ReservaException("Esa plaza ya no está disponible. Otro conductor la reservó primero.");

            // Bloqueamos la plaza dentro de la misma transacción.
            plaza.Estado = EstadoPlaza.reservada;

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                PlazaId = plaza.Id,
                HoraReserva = DateTime.UtcNow,
                HoraEstimadaArribo = request.HoraEstimadaArribo,
                Estado = EstadoReserva.pendiente,
                QrToken = _qr.GenerarTokenSeguro(),
                QrExpiraEn = request.HoraEstimadaArribo.Add(ToleranciaLlegada),
                CreatedAt = DateTime.UtcNow
            };

            _db.Reservas.Add(reserva);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ReservaResponseDto
            {
                Id = reserva.Id,
                PlazaCodigo = plaza.Codigo,
                ParqueaderoNombre = plaza.Parqueadero!.Nombre,
                HoraReserva = reserva.HoraReserva,
                HoraEstimadaArribo = reserva.HoraEstimadaArribo,
                Estado = reserva.Estado.ToString(),
                QrToken = reserva.QrToken,
                QrExpiraEn = reserva.QrExpiraEn
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // -----------------------------------------------------------------
    public async Task CancelarReservaAsync(int usuarioId, int reservaId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var reserva = await _db.Reservas
                .Include(r => r.Plaza)
                .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == usuarioId);

            if (reserva is null)
                throw new ReservaException("Reserva no encontrada.");

            if (reserva.Estado is EstadoReserva.completada or EstadoReserva.cancelada)
                throw new ReservaException("Esta reserva ya no se puede cancelar.");

            reserva.Estado = EstadoReserva.cancelada;

            // Solo liberamos la plaza si nadie más la ocupó físicamente todavía.
            if (reserva.Plaza!.Estado == EstadoPlaza.reservada)
                reserva.Plaza.Estado = EstadoPlaza.libre;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // -----------------------------------------------------------------
    public async Task<List<ReservaResponseDto>> MisReservasAsync(int usuarioId)
    {
        return await _db.Reservas
            .Where(r => r.UsuarioId == usuarioId)
            .Include(r => r.Plaza).ThenInclude(p => p!.Parqueadero)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReservaResponseDto
            {
                Id = r.Id,
                PlazaCodigo = r.Plaza!.Codigo,
                ParqueaderoNombre = r.Plaza.Parqueadero!.Nombre,
                HoraReserva = r.HoraReserva,
                HoraEstimadaArribo = r.HoraEstimadaArribo,
                Estado = r.Estado.ToString(),
                QrToken = r.QrToken,
                QrExpiraEn = r.QrExpiraEn
            })
            .ToListAsync();
    }

    // -----------------------------------------------------------------
    // VALIDAR ENTRADA - lo escanea el operador en la puerta
    // -----------------------------------------------------------------
    public async Task<ValidarAccesoResponseDto> ValidarEntradaAsync(int operadorId, string qrToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var reserva = await _db.Reservas
                .Include(r => r.Plaza)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.QrToken == qrToken);

            if (reserva is null)
                return new ValidarAccesoResponseDto { Valido = false, Mensaje = "Código QR no reconocido." };

            if (reserva.Estado != EstadoReserva.pendiente)
                return new ValidarAccesoResponseDto
                {
                    Valido = false,
                    Mensaje = $"Esta reserva ya no está pendiente (estado actual: {reserva.Estado})."
                };

            if (reserva.QrExpiraEn is not null && reserva.QrExpiraEn < DateTime.UtcNow)
            {
                reserva.Estado = EstadoReserva.expirada;
                if (reserva.Plaza!.Estado == EstadoPlaza.reservada)
                    reserva.Plaza.Estado = EstadoPlaza.libre;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ValidarAccesoResponseDto { Valido = false, Mensaje = "El QR de esta reserva ya expiró." };
            }

            // Todo correcto: confirmamos la reserva, ocupamos la plaza y registramos el acceso.
            reserva.Estado = EstadoReserva.confirmada;
            reserva.HoraLlegadaReal = DateTime.UtcNow;
            reserva.Plaza!.Estado = EstadoPlaza.ocupada;

            _db.Accesos.Add(new Acceso
            {
                ReservaId = reserva.Id,
                OperadorId = operadorId,
                Tipo = TipoAcceso.entrada,
                FechaHora = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ValidarAccesoResponseDto
            {
                Valido = true,
                Mensaje = "Acceso autorizado.",
                ConductorNombre = reserva.Usuario!.Nombre,
                PlazaCodigo = reserva.Plaza.Codigo,
                TipoAccesoRegistrado = "entrada"
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // -----------------------------------------------------------------
    // REGISTRAR SALIDA - libera la plaza y calcula el cobro
    // -----------------------------------------------------------------
    public async Task<RegistrarSalidaResponseDto> RegistrarSalidaAsync(int operadorId, string qrToken, string metodoPago)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var reserva = await _db.Reservas
                .Include(r => r.Plaza).ThenInclude(p => p!.Parqueadero).ThenInclude(pq => pq!.Tarifas)
                .FirstOrDefaultAsync(r => r.QrToken == qrToken);

            if (reserva is null)
                throw new ReservaException("Código QR no reconocido.");

            if (reserva.Estado != EstadoReserva.confirmada)
                throw new ReservaException("Esta reserva no tiene una entrada activa que cerrar.");

            reserva.HoraSalida = DateTime.UtcNow;
            reserva.Estado = EstadoReserva.completada;
            reserva.Plaza!.Estado = EstadoPlaza.libre;

            var duracion = reserva.HoraSalida.Value - reserva.HoraLlegadaReal!.Value;
            var duracionMinutos = Math.Max(1, (int)Math.Ceiling(duracion.TotalMinutes));

            var tarifa = reserva.Plaza.Parqueadero!.Tarifas
                .Where(t => t.TipoVehiculo == reserva.Plaza.TipoVehiculo)
                .OrderByDescending(t => t.VigenteDesde)
                .FirstOrDefault();

            var valorHora = tarifa?.ValorHora ?? 1.00m; // fallback conservador si no hay tarifa configurada
            var monto = Math.Round((decimal)duracionMinutos / 60m * valorHora, 2);

            _db.Accesos.Add(new Acceso
            {
                ReservaId = reserva.Id,
                OperadorId = operadorId,
                Tipo = TipoAcceso.salida,
                FechaHora = DateTime.UtcNow
            });

            _db.TransaccionesPago.Add(new TransaccionPago
            {
                ReservaId = reserva.Id,
                Monto = monto,
                MetodoPago = Enum.Parse<MetodoPago>(metodoPago.ToLower()),
                Estado = EstadoPago.pagado,
                FechaHora = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new RegistrarSalidaResponseDto
            {
                MontoCobrado = monto,
                DuracionMinutos = duracionMinutos,
                MetodoPago = metodoPago
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
