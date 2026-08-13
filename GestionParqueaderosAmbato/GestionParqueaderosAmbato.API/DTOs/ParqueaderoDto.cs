namespace GestionParqueaderosAmbato.API.DTOs
{
    public class ParqueaderoDto
    {
        public int IdParqueadero { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        public string? Telefono { get; set; }

        public string? HorarioAtencion { get; set; }

        public bool Estado { get; set; }
    }
}
