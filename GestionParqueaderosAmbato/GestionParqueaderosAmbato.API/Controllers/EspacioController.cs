using GestionParqueaderosAmbato.API.Data;
using GestionParqueaderosAmbato.API.DTOs;
using GestionParqueaderosAmbato.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionParqueaderosAmbato.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspaciosController : ControllerBase
    {
        private readonly GestionParqueaderosDbContext _context;

        public EspaciosController(GestionParqueaderosDbContext context)
        {
            _context = context;
        }

        // GET: api/Espacios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspacioDto>>> ObtenerEspacios()
        {
            var espacios = await _context.Espacios
                .Select(e => new EspacioDto
                {
                    IdEspacio = e.IdEspacio,
                    IdParqueadero = e.IdParqueadero,
                    NumeroEspacio = e.NumeroEspacio,
                    Estado = e.Estado,
                    Observacion = e.Observacion
                })
                .ToListAsync();

            return Ok(espacios);
        }

        // GET: api/Espacios/parqueadero/1
        [HttpGet("parqueadero/{idParqueadero}")]
        public async Task<ActionResult<IEnumerable<EspacioDto>>> ObtenerEspaciosPorParqueadero(int idParqueadero)
        {
            var espacios = await _context.Espacios
                .Where(e => e.IdParqueadero == idParqueadero)
                .Select(e => new EspacioDto
                {
                    IdEspacio = e.IdEspacio,
                    IdParqueadero = e.IdParqueadero,
                    NumeroEspacio = e.NumeroEspacio,
                    Estado = e.Estado,
                    Observacion = e.Observacion
                })
                .ToListAsync();

            return Ok(espacios);
        }

        // POST: api/Espacios
        [HttpPost]
        public async Task<ActionResult<EspacioDto>> CrearEspacio(EspacioDto dto)
        {
            var parqueaderoExiste = await _context.Parqueaderos
                .AnyAsync(p => p.IdParqueadero == dto.IdParqueadero);

            if (!parqueaderoExiste)
            {
                return BadRequest("El parqueadero indicado no existe.");
            }

            var espacio = new Espacio
            {
                IdParqueadero = dto.IdParqueadero,
                NumeroEspacio = dto.NumeroEspacio,
                Estado = dto.Estado,
                Observacion = dto.Observacion
            };

            _context.Espacios.Add(espacio);
            await _context.SaveChangesAsync();

            dto.IdEspacio = espacio.IdEspacio;

            return CreatedAtAction(
                nameof(ObtenerEspacios),
                new { id = espacio.IdEspacio },
                dto
            );
        }

        // PUT: api/Espacios/1
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEspacio(int id, EspacioDto dto)
        {
            if (id != dto.IdEspacio)
            {
                return BadRequest("El ID de la URL no coincide con el ID del espacio.");
            }

            var espacio = await _context.Espacios
                .FirstOrDefaultAsync(e => e.IdEspacio == id);

            if (espacio == null)
            {
                return NotFound("El espacio no existe.");
            }

            var parqueaderoExiste = await _context.Parqueaderos
                .AnyAsync(p => p.IdParqueadero == dto.IdParqueadero);

            if (!parqueaderoExiste)
            {
                return BadRequest("El parqueadero indicado no existe.");
            }

            espacio.IdParqueadero = dto.IdParqueadero;
            espacio.NumeroEspacio = dto.NumeroEspacio;
            espacio.Estado = dto.Estado;
            espacio.Observacion = dto.Observacion;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Espacios/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEspacio(int id)
        {
            var espacio = await _context.Espacios
                .FirstOrDefaultAsync(e => e.IdEspacio == id);

            if (espacio == null)
            {
                return NotFound("El espacio no existe.");
            }

            _context.Espacios.Remove(espacio);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}