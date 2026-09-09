namespace SmartParking.Api.Dtos.Dashboard;

public class KpiGeneralDto
{
    public int PlazasTotales { get; set; }
    public int PlazasOcupadas { get; set; }
    public int CuposLibres { get; set; }
    public int ReservasPendientesHoy { get; set; }
}

public class KpiPorParqueaderoDto
{
    public int ParqueaderoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int PlazasTotales { get; set; }
    public int PlazasOcupadas { get; set; }
    public int CuposLibres { get; set; }
    public int ReservasActivas { get; set; }
}

public class OcupacionHorariaDto
{
    public int Hora { get; set; } // 0-23
    public int Entradas { get; set; }
    public int Salidas { get; set; }
}

public class AlertaDto
{
    public string Tipo { get; set; } = string.Empty; // "parqueadero_lleno" | "reserva_vencida"
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

public class RecaudacionDiariaDto
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public int TransaccionesCount { get; set; }
}
