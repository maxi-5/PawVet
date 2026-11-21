using Microsoft.EntityFrameworkCore;
using PawVetClinica.Models;

namespace PawVetClinica.Services
{
    // Al iniciar: aplica las migraciones pendientes y crea el usuario Administrador inicial
    public static class DbSeeder
    {
        public static async Task InicializarAsync(IServiceProvider services, IConfiguration config, ILogger logger)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ClinicaDbContext>();

            // sin migraciones la base quedaria vacia: se avisa con un mensaje claro
            if (context.Database.IsRelational() && !context.Database.GetMigrations().Any())
            {
                throw new InvalidOperationException(
                    "No hay migraciones. Ejecuta una vez: Add-Migration Inicial -Project PawVetClinica -StartupProject PawVetClinica");
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
                    break;
                }
                catch (Exception ex) when (intento < 10)
                {
                    logger.LogWarning("La base de datos aun no responde (intento {Intento}/10): {Mensaje}", intento, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            var email = string.IsNullOrWhiteSpace(config["Admin:Email"]) ? "admin@pawvet.cl" : config["Admin:Email"]!;
            var password = string.IsNullOrWhiteSpace(config["Admin:Password"]) ? "Admin123!" : config["Admin:Password"]!;

            if (!await context.Usuarios.AnyAsync(u => u.Email == email))
            {
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Administrador PawVet",
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    IdRol = Rol.ADMINISTRADOR
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Usuario administrador creado: {Email}", email);
            }
        }
    }
}
