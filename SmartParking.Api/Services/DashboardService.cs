using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Dashboard;
using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public interface IDashboardService
{
    Task<KpiGeneralDto> ObtenerKpiGeneralAsync();
    Task<List<KpiPorParqueaderoDto>> ObtenerKpiPorParqueaderoAsync();
    Task<List<OcupacionHorariaDto>> ObtenerOcupacionHorariaHoyAsync();
    Task<List<AlertaDto>> ObtenerAlertasAsync();
    Task<RecaudacionDiariaDto> ObtenerRecaudacionHoyAsync();
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<KpiGeneralDto> ObtenerKpiGeneralAsync()
    {
        var kpisPorParqueadero = await _db.VistaKpiParqueadero.ToListAsync();

        var hoy = DateTime.UtcNow.Date;
        var reservasActivas = await _db.Reservas.CountAsync(r =>
            (r.Estado == EstadoReserva.pendiente || r.Estado == EstadoReserva.confirmada) &&
            r.HoraEstimadaArribo >= hoy &&
            r.HoraEstimadaArribo < hoy.AddDays(1));

        var plazasTotales = await _db.Plazas.CountAsync();
        var plazasOcupadas = await _db.Plazas.CountAsync(p => p.Estado == EstadoPlaza.ocupada);
        var cuposLibres = await _db.Plazas.CountAsync(p => p.Estado == EstadoPlaza.libre);

        return new KpiGeneralDto
        {
            PlazasTotales = plazasTotales > 0 ? plazasTotales : kpisPorParqueadero.Sum(k => k.PlazasTotales),
            PlazasOcupadas = plazasOcupadas > 0 ? plazasOcupadas : kpisPorParqueadero.Sum(k => k.PlazasOcupadas),
            CuposLibres = cuposLibres > 0 ? cuposLibres : kpisPorParqueadero.Sum(k => k.CuposLibres),
            ReservasPendientesHoy = reservasActivas
        };
    }

    public async Task<List<KpiPorParqueaderoDto>> ObtenerKpiPorParqueaderoAsync()
    {
        return await _db.VistaKpiParqueadero
            .Select(k => new KpiPorParqueaderoDto
            {
                ParqueaderoId = k.ParqueaderoId,
                Nombre = k.ParqueaderoNombre,
                PlazasTotales = k.PlazasTotales,
                PlazasOcupadas = k.PlazasOcupadas,
                CuposLibres = k.CuposLibres,
                ReservasActivas = k.ReservasActivas
            })
            .ToListAsync();
    }

    public async Task<List<OcupacionHorariaDto>> ObtenerOcupacionHorariaHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;

        var accesosHoy = await _db.Accesos
            .Where(a => a.FechaHora >= hoy && a.FechaHora < hoy.AddDays(1))
            .Select(a => new { a.Tipo, a.FechaHora.Hour })
            .ToListAsync();

        var resultado = Enumerable.Range(0, 24)
            .Select(hora => new OcupacionHorariaDto
            {
                Hora = hora,
                Entradas = accesosHoy.Count(a => a.Hour == hora && a.Tipo == TipoAcceso.entrada),
                Salidas = accesosHoy.Count(a => a.Hour == hora && a.Tipo == TipoAcceso.salida)
            })
            .ToList();

        return resultado;
    }

    public async Task<List<AlertaDto>> ObtenerAlertasAsync()
    {
        var alertas = new List<AlertaDto>();

        // Alerta 1: parqueaderos llenos (0 cupos libres)
        var llenos = await _db.VistaKpiParqueadero
            .Where(k => k.CuposLibres == 0 && k.PlazasTotales > 0)
            .ToListAsync();

        alertas.AddRange(llenos.Select(p => new AlertaDto
        {
            Tipo = "parqueadero_lleno",
            Mensaje = $"El parqueadero '{p.ParqueaderoNombre}' está lleno (0 cupos libres).",
            FechaHora = DateTime.UtcNow
        }));

        // Alerta 2: reservas vencidas no reclamadas (expiradas hoy)
        var hoy = DateTime.UtcNow.Date;
        var vencidasHoy = await _db.Reservas
            .Where(r => r.Estado == EstadoReserva.expirada &&
                        r.QrExpiraEn != null &&
                        r.QrExpiraEn >= hoy && r.QrExpiraEn < hoy.AddDays(1))
            .Include(r => r.Plaza)
            .ToListAsync();

        alertas.AddRange(vencidasHoy.Select(r => new AlertaDto
        {
            Tipo = "reserva_vencida",
            Mensaje = $"La reserva #{r.Id} para la plaza {r.Plaza!.Codigo} venció sin que el conductor se presentara.",
            FechaHora = r.QrExpiraEn!.Value
        }));

        return alertas.OrderByDescending(a => a.FechaHora).ToList();
    }

    public async Task<RecaudacionDiariaDto> ObtenerRecaudacionHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;

        var pagosHoy = await _db.TransaccionesPago
            .Where(t => t.Estado == EstadoPago.pagado &&
                        t.FechaHora >= hoy && t.FechaHora < hoy.AddDays(1))
            .ToListAsync();

        return new RecaudacionDiariaDto
        {
            Fecha = hoy,
            Total = pagosHoy.Sum(t => t.Monto),
            TransaccionesCount = pagosHoy.Count
        };
    }
}
