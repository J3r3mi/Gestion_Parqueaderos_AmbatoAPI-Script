using GestionParqueaderosAmbato.API.Data;
using GestionParqueaderosAmbato.API.DTOs;
using GestionParqueaderosAmbato.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestionParqueaderosAmbato.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly GestionParqueaderosDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UsuariosController(
            GestionParqueaderosDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // =====================================================
        // GET: api/Usuarios
        // Obtener todos los usuarios
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> ObtenerUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    IdRol = u.IdRol,
                    Nombres = u.Nombres,
                    Apellidos = u.Apellidos,
                    Cedula = u.Cedula,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    Estado = u.Estado,
                    FechaRegistro = u.FechaRegistro
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // =====================================================
        // GET: api/Usuarios/{id}
        // Obtener un usuario por ID
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    IdRol = u.IdRol,
                    Nombres = u.Nombres,
                    Apellidos = u.Apellidos,
                    Cedula = u.Cedula,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    Estado = u.Estado,
                    FechaRegistro = u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound("El usuario no existe.");
            }

            return Ok(usuario);
        }

        // =====================================================
        // POST: api/Usuarios/registro
        // Registrar un nuevo usuario
        // =====================================================
        [HttpPost("registro")]
        public async Task<ActionResult<UsuarioDto>> RegistrarUsuario(
            RegistroUsuarioDto dto)
        {
            // Verificar si la cédula ya está registrada
            var cedulaExiste = await _context.Usuarios
                .AnyAsync(u => u.Cedula == dto.Cedula);

            if (cedulaExiste)
            {
                return BadRequest("La cédula ya está registrada.");
            }

            // Verificar si el correo ya está registrado
            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo);

            if (correoExiste)
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            // Verificar que el rol exista
            var rolExiste = await _context.Roles
                .AnyAsync(r => r.IdRol == dto.IdRol);

            if (!rolExiste)
            {
                return BadRequest("El rol indicado no existe.");
            }

            // Crear el usuario
            var usuario = new Usuario
            {
                IdRol = dto.IdRol,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Cedula = dto.Cedula,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            // Generar hash seguro de la contraseña
            usuario.PasswordHash = _passwordHasher.HashPassword(
                usuario,
                dto.Password
            );

            // Guardar usuario
            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            // Crear respuesta sin PasswordHash
            var usuarioDto = new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Cedula = usuario.Cedula,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                Estado = usuario.Estado,
                FechaRegistro = usuario.FechaRegistro
            };

            return CreatedAtAction(
                nameof(ObtenerUsuario),
                new { id = usuario.IdUsuario },
                usuarioDto
            );
        }

        // =====================================================
        // POST: api/Usuarios/login
        // Iniciar sesión y generar JWT
        // =====================================================
        [HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> IniciarSesion(
            LoginDto dto)
        {
            // Buscar usuario por correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            // Verificar que exista
            if (usuario == null)
            {
                return Unauthorized(
                    "El correo o la contraseña son incorrectos."
                );
            }

            // Verificar que esté activo
            if (!usuario.Estado)
            {
                return Unauthorized(
                    "El usuario se encuentra inactivo."
                );
            }

            // Verificar contraseña
            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                dto.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "El correo o la contraseña son incorrectos."
                );
            }

            // =================================================
            // Crear los Claims del usuario
            // =================================================
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombres
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.IdRol.ToString()
                )
            };

            // Obtener configuración JWT
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) ||
                string.IsNullOrEmpty(issuer) ||
                string.IsNullOrEmpty(audience))
            {
                return StatusCode(
                    500,
                    "La configuración JWT no está completa."
                );
            }

            // Crear clave de seguridad
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            // Tiempo de expiración
            var expirationMinutes =
                _configuration.GetValue<int>(
                    "Jwt:ExpirationMinutes"
                );

            // Crear token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    expirationMinutes
                ),
                signingCredentials: credentials
            );

            // Convertir token a texto
            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // =================================================
            // Crear información del usuario
            // =================================================
            var usuarioDto = new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Cedula = usuario.Cedula,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                Estado = usuario.Estado,
                FechaRegistro = usuario.FechaRegistro
            };

            // =================================================
            // Crear respuesta final
            // =================================================
            var respuesta = new LoginRespuestaDto
            {
                Token = tokenString,
                Usuario = usuarioDto
            };

            return Ok(respuesta);
        }
    }
}