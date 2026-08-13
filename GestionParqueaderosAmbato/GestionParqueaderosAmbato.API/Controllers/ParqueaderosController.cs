using GestionParqueaderosAmbato.API.Data;
using GestionParqueaderosAmbato.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionParqueaderosAmbato.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParqueaderosController : ControllerBase
    {
        private readonly GestionParqueaderosDbContext _context;

        public ParqueaderosController(GestionParqueaderosDbContext context)
        {
            _context = context;
        }

        // GET: api/Parqueaderos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParqueaderoDto>>> ObtenerParqueaderos()
        {
            var parqueaderos = await _context.Parqueaderos
                .Select(p => new ParqueaderoDto
                {
                    IdParqueadero = p.IdParqueadero,
                    Nombre = p.Nombre,
                    Direccion = p.Direccion,
                    Latitud = p.Latitud,
                    Longitud = p.Longitud,
                    Telefono = p.Telefono,
                    HorarioAtencion = p.HorarioAtencion,
                    Estado = p.Estado
                })
                .ToListAsync();

            return Ok(parqueaderos);
        }
    }
}