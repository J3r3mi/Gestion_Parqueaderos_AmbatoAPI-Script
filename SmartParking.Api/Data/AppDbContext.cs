using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Models;

namespace SmartParking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Parqueadero> Parqueaderos => Set<Parqueadero>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    public DbSet<Plaza> Plazas => Set<Plaza>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Acceso> Accesos => Set<Acceso>();
    public DbSet<TransaccionPago> TransaccionesPago => Set<TransaccionPago>();
    public DbSet<VwKpiParqueadero> VistaKpiParqueadero => Set<VwKpiParqueadero>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------------- USUARIOS ----------------
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Cedula).HasColumnName("cedula");
            e.Property(x => x.Correo).HasColumnName("correo");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.Rol).HasColumnName("rol")
                .HasConversion<string>();
            e.Property(x => x.Activo).HasColumnName("activo");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Cedula).IsUnique();
            e.HasIndex(x => x.Correo).IsUnique();
        });

        // ---------------- PARQUEADEROS ----------------
        modelBuilder.Entity<Parqueadero>(e =>
        {
            e.ToTable("parqueaderos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre");
            e.Property(x => x.Direccion).HasColumnName("direccion");
            e.Property(x => x.Latitud).HasColumnName("latitud").HasColumnType("decimal(10,7)");
            e.Property(x => x.Longitud).HasColumnName("longitud").HasColumnType("decimal(10,7)");
            e.Property(x => x.CapacidadTotal).HasColumnName("capacidad_total");
            e.Property(x => x.AdministradorId).HasColumnName("administrador_id");
            e.Property(x => x.Activo).HasColumnName("activo");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasOne(x => x.Administrador)
                .WithMany()
                .HasForeignKey(x => x.AdministradorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------------- TARIFAS ----------------
        modelBuilder.Entity<Tarifa>(e =>
        {
            e.ToTable("tarifas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ParqueaderoId).HasColumnName("parqueadero_id");
            e.Property(x => x.TipoVehiculo).HasColumnName("tipo_vehiculo").HasConversion<string>();
            e.Property(x => x.ValorHora).HasColumnName("valor_hora").HasColumnType("decimal(6,2)");
            e.Property(x => x.VigenteDesde).HasColumnName("vigente_desde");

            e.HasOne(x => x.Parqueadero)
                .WithMany(p => p.Tarifas)
                .HasForeignKey(x => x.ParqueaderoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- PASSWORD_RESET_TOKENS ----------------
        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("password_reset_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            e.Property(x => x.Token).HasColumnName("token");
            e.Property(x => x.ExpiraEn).HasColumnName("expira_en");
            e.Property(x => x.Usado).HasColumnName("usado");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasIndex(x => x.Token).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- PLAZAS ----------------
        modelBuilder.Entity<Plaza>(e =>
        {
            e.ToTable("plazas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ParqueaderoId).HasColumnName("parqueadero_id");
            e.Property(x => x.Codigo).HasColumnName("codigo");
            e.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>();
            e.Property(x => x.TipoVehiculo).HasColumnName("tipo_vehiculo").HasConversion<string>();
            e.Property(x => x.Version).HasColumnName("version");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasIndex(x => new { x.ParqueaderoId, x.Codigo }).IsUnique();
            e.HasIndex(x => new { x.ParqueaderoId, x.Estado });

            e.HasOne(x => x.Parqueadero)
                .WithMany(p => p.Plazas)
                .HasForeignKey(x => x.ParqueaderoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- RESERVAS ----------------
        modelBuilder.Entity<Reserva>(e =>
        {
            e.ToTable("reservas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            e.Property(x => x.PlazaId).HasColumnName("plaza_id");
            e.Property(x => x.HoraReserva).HasColumnName("hora_reserva");
            e.Property(x => x.HoraEstimadaArribo).HasColumnName("hora_estimada_arribo");
            e.Property(x => x.HoraLlegadaReal).HasColumnName("hora_llegada_real");
            e.Property(x => x.HoraSalida).HasColumnName("hora_salida");
            e.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>();
            e.Property(x => x.QrToken).HasColumnName("qr_token");
            e.Property(x => x.QrExpiraEn).HasColumnName("qr_expira_en");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasIndex(x => x.QrToken).IsUnique();
            e.HasIndex(x => x.Estado);
            e.HasIndex(x => x.HoraEstimadaArribo);

            e.HasOne(x => x.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Plaza)
                .WithMany(p => p.Reservas)
                .HasForeignKey(x => x.PlazaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- ACCESOS ----------------
        modelBuilder.Entity<Acceso>(e =>
        {
            e.ToTable("accesos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ReservaId).HasColumnName("reserva_id");
            e.Property(x => x.OperadorId).HasColumnName("operador_id");
            e.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>();
            e.Property(x => x.FechaHora).HasColumnName("fecha_hora");

            e.HasOne(x => x.Reserva)
                .WithMany(r => r.Accesos)
                .HasForeignKey(x => x.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Operador)
                .WithMany()
                .HasForeignKey(x => x.OperadorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------------- TRANSACCIONES DE PAGO ----------------
        modelBuilder.Entity<TransaccionPago>(e =>
        {
            e.ToTable("transacciones_pago");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ReservaId).HasColumnName("reserva_id");
            e.Property(x => x.Monto).HasColumnName("monto").HasColumnType("decimal(8,2)");
            e.Property(x => x.MetodoPago).HasColumnName("metodo_pago").HasConversion<string>();
            e.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>();
            e.Property(x => x.FechaHora).HasColumnName("fecha_hora");

            e.HasOne(x => x.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(x => x.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- VISTA vw_kpi_parqueadero (solo lectura) ----------------
        modelBuilder.Entity<VwKpiParqueadero>(e =>
        {
            e.HasNoKey();
            e.ToView("vw_kpi_parqueadero");
            e.Property(x => x.ParqueaderoId).HasColumnName("parqueadero_id");
            e.Property(x => x.ParqueaderoNombre).HasColumnName("parqueadero_nombre");
            e.Property(x => x.PlazasTotales).HasColumnName("plazas_totales");
            e.Property(x => x.PlazasOcupadas).HasColumnName("plazas_ocupadas");
            e.Property(x => x.CuposLibres).HasColumnName("cupos_libres");
            e.Property(x => x.ReservasActivas).HasColumnName("reservas_activas");
        });
    }
}
