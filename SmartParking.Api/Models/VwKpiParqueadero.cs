namespace SmartParking.Api.Models;

/// <summary>
/// Mapea 1:1 la vista `vw_kpi_parqueadero` definida en schema.sql.
/// Es "keyless" porque es una vista de solo lectura, no una tabla con PK propia.
/// </summary>
public class VwKpiParqueadero
{
    public int ParqueaderoId { get; set; }
    public string ParqueaderoNombre { get; set; } = string.Empty;
    public int PlazasTotales { get; set; }
    public int PlazasOcupadas { get; set; }
    public int CuposLibres { get; set; }
    public int ReservasActivas { get; set; }
}
