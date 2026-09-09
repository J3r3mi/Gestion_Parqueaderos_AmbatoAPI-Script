using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Parqueaderos;
using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public class ParqueaderoException : Exception
{
    public ParqueaderoException(string mensaje) : base(mensaje) { }
}

public interface IParqueaderoService
{
    Task<List<ParqueaderoListItemDto>> ListarActivosAsync();
    Task<List<PlazaDto>> ListarPlazasAsync(int parqueaderoId);
    Task<List<TarifaDto>> ListarTarifasVigentesAsync(int parqueaderoId);

    Task<int> CrearParqueaderoAsync(int administradorId, CrearParqueaderoRequestDto request);
    Task ActualizarParqueaderoAsync(int parqueaderoId, ActualizarParqueaderoRequestDto request);
    Task<int> AgregarPlazaAsync(int parqueaderoId, CrearPlazaRequestDto request);
    Task ActualizarEstadoPlazaAsync(int parqueaderoId, int plazaId, ActualizarEstadoPlazaRequestDto request);
    Task AgregarTarifaAsync(int parqueaderoId, CrearTarifaRequestDto request);
}

public class ParqueaderoService : IParqueaderoService
{
    private readonly AppDbContext _db;

    public ParqueaderoService(AppDbContext db)
    {
        _db = db;
    }

    // -----------------------------------------------------------------
    // LECTURA - usados por el mapa y el flujo de reserva del conductor
    // -----------------------------------------------------------------
    public async Task<List<ParqueaderoListItemDto>> ListarActivosAsync()
    {
        return await _db.Parqueaderos
            .Where(p => p.Activo)
            .Select(p => new ParqueaderoListItemDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Direccion = p.Direccion,
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                CapacidadTotal = p.CapacidadTotal,
                CuposLibres = p.Plazas.Count(pl => pl.Estado == EstadoPlaza.libre)
            })
            .ToListAsync();
    }

    public async Task<List<PlazaDto>> ListarPlazasAsync(int parqueaderoId)
    {
        return await _db.Plazas
            .Where(pl => pl.ParqueaderoId == parqueaderoId)
            .OrderBy(pl => pl.Codigo)
            .Select(pl => new PlazaDto
            {
                Id = pl.Id,
                Codigo = pl.Codigo,
                Estado = pl.Estado.ToString(),
                TipoVehiculo = pl.TipoVehiculo.ToString()
            })
            .ToListAsync();
    }

    public async Task<List<TarifaDto>> ListarTarifasVigentesAsync(int parqueaderoId)
    {
        // Para cada tipo de vehículo, solo la tarifa más reciente (misma regla que usa
        // ReservaService al calcular el cobro de salida).
        var tarifas = await _db.Tarifas
            .Where(t => t.ParqueaderoId == parqueaderoId)
            .ToListAsync();

        return tarifas
            .GroupBy(t => t.TipoVehiculo)
            .Select(g => g.OrderByDescending(t => t.VigenteDesde).First())
            .Select(t => new TarifaDto
            {
                Id = t.Id,
                TipoVehiculo = t.TipoVehiculo.ToString(),
                ValorHora = t.ValorHora,
                VigenteDesde = t.VigenteDesde
            })
            .OrderBy(t => t.TipoVehiculo)
            .ToList();
    }

    // -----------------------------------------------------------------
    // ESCRITURA - solo Administrador
    // -----------------------------------------------------------------
    public async Task<int> CrearParqueaderoAsync(int administradorId, CrearParqueaderoRequestDto request)
    {
        var parqueadero = new Parqueadero
        {
            Nombre = request.Nombre,
            Direccion = request.Direccion,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            CapacidadTotal = request.CapacidadTotal,
            AdministradorId = administradorId,
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Parqueaderos.Add(parqueadero);
        await _db.SaveChangesAsync();
        return parqueadero.Id;
    }

    public async Task ActualizarParqueaderoAsync(int parqueaderoId, ActualizarParqueaderoRequestDto request)
    {
        var parqueadero = await _db.Parqueaderos.FindAsync(parqueaderoId)
            ?? throw new ParqueaderoException("Parqueadero no encontrado.");

        parqueadero.Nombre = request.Nombre;
        parqueadero.Direccion = request.Direccion;
        parqueadero.Latitud = request.Latitud;
        parqueadero.Longitud = request.Longitud;
        parqueadero.Activo = request.Activo;

        await _db.SaveChangesAsync();
    }

    public async Task<int> AgregarPlazaAsync(int parqueaderoId, CrearPlazaRequestDto request)
    {
        var existeParqueadero = await _db.Parqueaderos.AnyAsync(p => p.Id == parqueaderoId);
        if (!existeParqueadero)
            throw new ParqueaderoException("Parqueadero no encontrado.");

        var codigoDuplicado = await _db.Plazas.AnyAsync(pl =>
            pl.ParqueaderoId == parqueaderoId && pl.Codigo == request.Codigo);
        if (codigoDuplicado)
            throw new ParqueaderoException($"Ya existe una plaza con el código '{request.Codigo}' en este parqueadero.");

        var plaza = new Plaza
        {
            ParqueaderoId = parqueaderoId,
            Codigo = request.Codigo,
            TipoVehiculo = request.TipoVehiculo,
            Estado = EstadoPlaza.libre,
            CreatedAt = DateTime.UtcNow
        };

        _db.Plazas.Add(plaza);
        await _db.SaveChangesAsync();
        return plaza.Id;
    }

    public async Task ActualizarEstadoPlazaAsync(int parqueaderoId, int plazaId, ActualizarEstadoPlazaRequestDto request)
    {
        // Nota: esto es para uso administrativo (ej. marcar 'mantenimiento').
        // El cambio de estado libre<->reservada<->ocupada durante el flujo normal
        // lo maneja ReservaService con locking pesimista, NO este endpoint.
        var plaza = await _db.Plazas
            .FirstOrDefaultAsync(pl => pl.Id == plazaId && pl.ParqueaderoId == parqueaderoId)
            ?? throw new ParqueaderoException("Plaza no encontrada en ese parqueadero.");

        if (plaza.Estado is EstadoPlaza.reservada or EstadoPlaza.ocupada &&
            request.Estado is EstadoPlaza.libre or EstadoPlaza.mantenimiento)
        {
            throw new ParqueaderoException(
                "No se puede cambiar el estado de una plaza reservada u ocupada directamente; " +
                "espera a que se complete o cancele la reserva activa.");
        }

        plaza.Estado = request.Estado;
        await _db.SaveChangesAsync();
    }

    public async Task AgregarTarifaAsync(int parqueaderoId, CrearTarifaRequestDto request)
    {
        var existeParqueadero = await _db.Parqueaderos.AnyAsync(p => p.Id == parqueaderoId);
        if (!existeParqueadero)
            throw new ParqueaderoException("Parqueadero no encontrado.");

        // Nunca se actualiza una tarifa existente: se inserta una nueva "vigente desde ahora".
        // Así el histórico de precios queda intacto (ver ReservaService.RegistrarSalidaAsync).
        var tarifa = new Tarifa
        {
            ParqueaderoId = parqueaderoId,
            TipoVehiculo = request.TipoVehiculo,
            ValorHora = request.ValorHora,
            VigenteDesde = DateTime.UtcNow
        };

        _db.Tarifas.Add(tarifa);
        await _db.SaveChangesAsync();
    }
}
