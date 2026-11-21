using System.Net;
using System.Net.Http.Json;
using PawVetClinica.DTOs;

namespace PawVetClinica.Tests
{
    // Veterinarios, especies y razas (CU-03)
    public class CatalogosTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public CatalogosTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetVeterinarios_ComoRecepcionista_Devuelve200()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var client = _factory.CreateClient();
            await client.ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);

            var resp = await client.GetAsync("/api/v1/veterinarios");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task PostVeterinario_ComoRecepcionista_Devuelve403()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var client = _factory.CreateClient();
            await client.ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);

            var resp = await client.PostAsJsonAsync("/api/v1/veterinarios", new VeterinarioGuardar { Rut = TestHelpers.RutValido(), Nombre = "Camila Soto" });

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostVeterinario_ConRutRepetido_Devuelve409()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var dto = new VeterinarioGuardar { Rut = TestHelpers.RutValido(), Nombre = "Camila Soto" };
            await admin.PostAsJsonAsync("/api/v1/veterinarios", dto);

            var resp = await admin.PostAsJsonAsync("/api/v1/veterinarios", dto);

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostVeterinario_VinculadoAUsuarioQueNoEsVeterinario_Devuelve400()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var recepcionista = await _factory.CreateClient().ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);

            var resp = await admin.PostAsJsonAsync("/api/v1/veterinarios",
                new VeterinarioGuardar { Rut = TestHelpers.RutValido(), Nombre = "Camila Soto", IdUser = recepcionista.IdUser });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetRazas_PorEspecie_SoloTraeEsaEspecie()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();

            var razas = await admin.GetFromJsonAsync<List<RazaDto>>("/api/v1/razas?idEspecie=2");

            Assert.NotEmpty(razas!);
            Assert.All(razas!, r => Assert.Equal(2, r.IdEspecie));
        }

        [Fact]
        public async Task PostRaza_ConEspecieInexistente_Devuelve400()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();

            var resp = await admin.PostAsJsonAsync("/api/v1/razas", new RazaCrear { Nombre = "Golden", IdEspecie = 999 });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteEspecie_ConRazas_Devuelve409()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();

            var resp = await admin.DeleteAsync("/api/v1/especies/1");

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostEspecie_Repetida_Devuelve409()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();

            var resp = await admin.PostAsJsonAsync("/api/v1/especies", new EspecieCrear { Nombre = "Perro" });

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }
    }
}
