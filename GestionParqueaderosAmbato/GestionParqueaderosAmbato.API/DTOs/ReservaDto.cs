namespace GestionParqueaderosAmbato.API.DTOs
{
    public class ReservaDto
    {
        public int IdReserva { get; set; }

        public int IdUsuario { get; set; }

        public int IdEspacio { get; set; }

        public DateTime FechaReserva { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
