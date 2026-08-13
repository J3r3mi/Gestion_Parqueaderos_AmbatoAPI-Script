namespace GestionParqueaderosAmbato.API.DTOs
{
    public class EspacioDto
    {
        public int IdEspacio { get; set; }

        public int IdParqueadero { get; set; }

        public string NumeroEspacio { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string? Observacion { get; set; }
    }
}
