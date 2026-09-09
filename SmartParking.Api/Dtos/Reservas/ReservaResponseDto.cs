namespace SmartParking.Api.Dtos.Reservas;

public class ReservaResponseDto
{
    public int Id { get; set; }
    public string PlazaCodigo { get; set; } = string.Empty;
    public string ParqueaderoNombre { get; set; } = string.Empty;
    public DateTime HoraReserva { get; set; }
    public DateTime HoraEstimadaArribo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? QrToken { get; set; }
    public DateTime? QrExpiraEn { get; set; }
}

public class ValidarAccesoRequestDto
{
    public string QrToken { get; set; } = string.Empty;
}

public class ValidarAccesoResponseDto
{
    public bool Valido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? ConductorNombre { get; set; }
    public string? PlazaCodigo { get; set; }
    public string? TipoAccesoRegistrado { get; set; } // "entrada" | "salida"
}

public class RegistrarSalidaResponseDto
{
    public decimal MontoCobrado { get; set; }
    public int DuracionMinutos { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
}
