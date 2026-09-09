using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "administrador")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Las 4 tarjetas superiores: Plazas Totales, Ocupadas, Cupos Libres, Reservas Pendientes.</summary>
    [HttpGet("kpis")]
    public async Task<IActionResult> Kpis()
    {
        var kpi = await _dashboardService.ObtenerKpiGeneralAsync();
        return Ok(kpi);
    }

    /// <summary>Mismo desglose pero por parqueadero, útil si hay más de uno.</summary>
    [HttpGet("kpis-por-parqueadero")]
    public async Task<IActionResult> KpisPorParqueadero()
    {
        var kpis = await _dashboardService.ObtenerKpiPorParqueaderoAsync();
        return Ok(kpis);
    }

    /// <summary>Datos para la gráfica de ocupación: entradas/salidas por hora del día actual.</summary>
    [HttpGet("ocupacion-horaria")]
    public async Task<IActionResult> OcupacionHoraria()
    {
        var datos = await _dashboardService.ObtenerOcupacionHorariaHoyAsync();
        return Ok(datos);
    }

    /// <summary>Parqueaderos llenos + reservas vencidas no reclamadas.</summary>
    [HttpGet("alertas")]
    public async Task<IActionResult> Alertas()
    {
        var alertas = await _dashboardService.ObtenerAlertasAsync();
        return Ok(alertas);
    }

    /// <summary>Total recaudado en el día actual.</summary>
    [HttpGet("recaudacion")]
    public async Task<IActionResult> Recaudacion()
    {
        var recaudacion = await _dashboardService.ObtenerRecaudacionHoyAsync();
        return Ok(recaudacion);
    }
}
