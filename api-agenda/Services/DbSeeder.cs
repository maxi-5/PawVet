using Microsoft.EntityFrameworkCore;
using PawVetAgenda.Models;

namespace PawVetAgenda.Services
{
    // Al iniciar: aplica las migraciones pendientes (las vacunas se cargan como datos iniciales)
    public static class DbSeeder
    {
        public static async Task InicializarAsync(IServiceProvider services, ILogger logger)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

            // sin migraciones la base quedaria vacia: se avisa con un mensaje claro
            if (context.Database.IsRelational() && !context.Database.GetMigrations().Any())
            {
                throw new InvalidOperationException(
                    "No hay migraciones. Ejecuta una vez: Add-Migration Inicial -Project PawVetAgenda -StartupProject PawVetAgenda");
            }

            // en Docker, SQL Server puede tardar en estar listo: se reintenta
            for (int intento = 1; intento <= 10; intento++)
            {
                try
                {
                    if (context.Database.IsRelational())
                        await context.Database.MigrateAsync();
                    else
                        await context.Database.EnsureCreatedAsync(); // pruebas con base en memoria
                    return;
                }
                catch (Exception ex) when (intento < 10)
                {
                    logger.LogWarning("La base de datos aun no responde (intento {Intento}/10): {Mensaje}", intento, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
