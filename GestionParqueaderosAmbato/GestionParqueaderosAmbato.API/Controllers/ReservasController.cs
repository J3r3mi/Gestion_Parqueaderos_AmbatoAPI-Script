using GestionParqueaderosAmbato.API.Data;
using GestionParqueaderosAmbato.API.DTOs;
using GestionParqueaderosAmbato.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionParqueaderosAmbato.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly GestionParqueaderosDbContext _context;

        public ReservasController(GestionParqueaderosDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> ObtenerReservas()
        {
            var reservas = await _context.Reservas
                .Select(r => new ReservaDto
                {
                    IdReserva = r.IdReserva,
                    IdUsuario = r.IdUsuario,
                    IdEspacio = r.IdEspacio,
                    FechaReserva = r.FechaReserva,
                    HoraInicio = r.HoraInicio,
                    HoraFin = r.HoraFin,
                    Estado = r.Estado,
                    
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/Reservas/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDto>> ObtenerReserva(int id)
        {
            var reserva = await _context.Reservas
                .Where(r => r.IdReserva == id)
                .Select(r => new ReservaDto
                {
                    IdReserva = r.IdReserva,
                    IdUsuario = r.IdUsuario,
                    IdEspacio = r.IdEspacio,
                    FechaReserva = r.FechaReserva,
                    HoraInicio = r.HoraInicio,
                    HoraFin = r.HoraFin,
                    Estado = r.Estado,
                    
                })
                .FirstOrDefaultAsync();

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            return Ok(reserva);
        }

        // GET: api/Reservas/usuario/1
        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> ObtenerReservasPorUsuario(int idUsuario)
        {
            var reservas = await _context.Reservas
                .Where(r => r.IdUsuario == idUsuario)
                .Select(r => new ReservaDto
                {
                    IdReserva = r.IdReserva,
                    IdUsuario = r.IdUsuario,
                    IdEspacio = r.IdEspacio,
                    FechaReserva = r.FechaReserva,
                    HoraInicio = r.HoraInicio,
                    HoraFin = r.HoraFin,
                    Estado = r.Estado,
                    
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<ActionResult<ReservaDto>> CrearReserva(ReservaDto dto)
        {
            // Verificar que el usuario exista
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == dto.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest("El usuario indicado no existe.");
            }

            // Verificar que el espacio exista
            var espacio = await _context.Espacios
                .FirstOrDefaultAsync(e => e.IdEspacio == dto.IdEspacio);

            if (espacio == null)
            {
                return BadRequest("El espacio indicado no existe.");
            }

            // Verificar que el horario sea válido
            if (dto.HoraInicio >= dto.HoraFin)
            {
                return BadRequest("La hora de inicio debe ser menor que la hora de fin.");
            }

            // Verificar que el espacio no esté ocupado
            if (espacio.Estado == "Ocupado")
            {
                return BadRequest("El espacio se encuentra ocupado.");
            }

            // Verificar que no exista otra reserva activa
            // en el mismo espacio, fecha y horario
            var reservaExistente = await _context.Reservas
                .AnyAsync(r =>
                    r.IdEspacio == dto.IdEspacio &&
                    r.FechaReserva.Date == dto.FechaReserva.Date &&
                    r.Estado != "Cancelada" &&
                    dto.HoraInicio < r.HoraFin &&
                    dto.HoraFin > r.HoraInicio
                );

            if (reservaExistente)
            {
                return BadRequest(
                    "El espacio ya tiene una reserva para la fecha y horario seleccionado."
                );
            }

            var reserva = new Reserva
            {
                IdUsuario = dto.IdUsuario,
                IdEspacio = dto.IdEspacio,
                FechaReserva = dto.FechaReserva,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Estado = "Confirmada",
                
            };

            _context.Reservas.Add(reserva);

            // Actualizar estado del espacio
            espacio.Estado = "Reservado";

            await _context.SaveChangesAsync();

            dto.IdReserva = reserva.IdReserva;
            dto.Estado = reserva.Estado;

            return CreatedAtAction(
                nameof(ObtenerReserva),
                new { id = reserva.IdReserva },
                dto
            );
        }

        // PUT: api/Reservas/1
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarReserva(
            int id,
            ReservaDto dto)
        {
            if (id != dto.IdReserva)
            {
                return BadRequest(
                    "El ID de la URL no coincide con el ID de la reserva."
                );
            }

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            reserva.FechaReserva = dto.FechaReserva;
            reserva.HoraInicio = dto.HoraInicio;
            reserva.HoraFin = dto.HoraFin;
            reserva.Estado = dto.Estado;
            

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Reservas/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound("La reserva no existe.");
            }

            // Cancelación lógica
            reserva.Estado = "Cancelada";

            // Buscar el espacio asociado
            var espacio = await _context.Espacios
                .FirstOrDefaultAsync(e => e.IdEspacio == reserva.IdEspacio);

            if (espacio != null)
            {
                espacio.Estado = "Disponible";
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}