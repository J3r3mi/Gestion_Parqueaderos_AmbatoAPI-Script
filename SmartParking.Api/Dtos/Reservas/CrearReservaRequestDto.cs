using System.ComponentModel.DataAnnotations;

namespace SmartParking.Api.Dtos.Reservas;

public class CrearReservaRequestDto
{
    [Required]
    public int PlazaId { get; set; }

    [Required]
    public DateTime HoraEstimadaArribo { get; set; }
}
