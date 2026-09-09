using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Models;

namespace SmartParking.Api.Data;

public static class DbInitializer
{
    public static void Initialize(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // Verifica si se puede conectar a la base de datos MySQL
            if (context.Database.CanConnect())
            {
                var usuariosConPlaceholder = context.Usuarios
                    .Where(u => u.PasswordHash == "$2a$11$PLACEHOLDER_HASH")
                    .ToList();

                if (usuariosConPlaceholder.Any())
                {
                    logger.LogInformation("Actualizando contraseñas placeholder de usuarios demo a 'Demo1234'...");
                    string hashValido = BCrypt.Net.BCrypt.HashPassword("Demo1234");

                    foreach (var usuario in usuariosConPlaceholder)
                    {
                        usuario.PasswordHash = hashValido;
                        usuario.UpdatedAt = DateTime.UtcNow;
                    }

                    context.SaveChanges();
                    logger.LogInformation("Contraseñas de usuarios demo actualizadas exitosamente a 'Demo1234'.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Aviso: No se pudo verificar usuarios placeholder. Asegúrate de haber importado schema.sql en MySQL.");
        }
    }
}
