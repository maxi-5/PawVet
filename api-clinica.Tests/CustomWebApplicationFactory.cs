using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PawVetClinica.Models;

namespace PawVetClinica.Tests
{
    // Levanta la API completa en memoria (middleware, JWT, roles, controladores, repositorios)
    // con una base de datos InMemory para no tocar la base real.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _nombreBd = "ClinicaTest_" + Guid.NewGuid();

        public CustomWebApplicationFactory()
        {
            // llave de prueba (en la app real viene de la variable de entorno Jwt__Key)
            Environment.SetEnvironmentVariable("Jwt__Key", "llave_de_pruebas_integracion_pawvet_2026!!!!");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<ClinicaDbContext>(options =>
                    options.UseInMemoryDatabase(_nombreBd));
            });
        }
    }
}
