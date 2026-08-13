using GestionParqueaderosAmbato.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionParqueaderosAmbato.API.Data
{
    public class GestionParqueaderosDbContext : DbContext
    {
        public GestionParqueaderosDbContext(
            DbContextOptions<GestionParqueaderosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Parqueadero> Parqueaderos { get; set; }

        public DbSet<Espacio> Espacios { get; set; }

        public DbSet<Reserva> Reservas { get; set; }

        public DbSet<HistorialReserva> HistorialReservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Rol -> Usuarios
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Usuario (Administrador) -> Parqueaderos
            modelBuilder.Entity<Parqueadero>()
                .HasOne(p => p.Administrador)
                .WithMany(u => u.Parqueaderos)
                .HasForeignKey(p => p.IdAdministrador)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Parqueadero -> Espacios
            modelBuilder.Entity<Espacio>()
                .HasOne(e => e.Parqueadero)
                .WithMany(p => p.Espacios)
                .HasForeignKey(e => e.IdParqueadero)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Usuario -> Reservas
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Espacio -> Reservas
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Espacio)
                .WithMany(e => e.Reservas)
                .HasForeignKey(r => r.IdEspacio)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Reserva -> Historial
            modelBuilder.Entity<HistorialReserva>()
                .HasOne(h => h.Reserva)
                .WithMany(r => r.Historial)
                .HasForeignKey(h => h.IdReserva)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}