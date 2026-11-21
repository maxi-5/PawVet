using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PawVetAgenda.Models;
using PawVetAgenda.Services;

namespace PawVetAgenda.Tests
{
    // Levanta la API Agenda en memoria, con base InMemory y con la API Clinica
    // y la API de feriados reemplazadas por clientes falsos.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _nombreBd = "AgendaTest_" + Guid.NewGuid();

        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("Jwt__Key", TestHelpers.LlaveJwt);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<AgendaDbContext>(options =>
                    options.UseInMemoryDatabase(_nombreBd));

                services.RemoveAll<IClinicaClient>();
                services.AddSingleton<IClinicaClient, FakeClinicaClient>();
                services.RemoveAll<IFeriadosClient>();
                services.AddSingleton<IFeriadosClient, FakeFeriadosClient>();
            });
        }
    }
}
