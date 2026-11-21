using System.Net;
using System.Net.Http.Json;
using PawVetClinica.DTOs;

namespace PawVetClinica.Tests
{
    // Tutores (RF-03) y mascotas (RF-04, RF-05, RF-06, HU-02)
    public class TutoresYMascotasTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TutoresYMascotasTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> RecepcionistaAsync()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var client = _factory.CreateClient();
            await client.ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);
            return client;
        }

        [Fact]
        public async Task PostTutor_ComoRecepcionista_Devuelve201()
        {
            var client = await RecepcionistaAsync();

            var resp = await client.PostAsJsonAsync("/api/v1/tutores", TestHelpers.TutorValido());

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            Assert.NotNull(resp.Headers.Location);
        }

        [Fact]
        public async Task PostTutor_ComoVeterinario_Devuelve403()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var client = _factory.CreateClient();
            await client.ComoNuevoUsuarioAsync(admin, TestHelpers.RolVeterinario);

            var resp = await client.PostAsJsonAsync("/api/v1/tutores", TestHelpers.TutorValido());

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostTutor_ConRutInvalido_Devuelve400()
        {
            var client = await RecepcionistaAsync();
            var dto = TestHelpers.TutorValido();
            dto.Rut = "12345678-9"; // digito verificador incorrecto (el correcto es 5)

            var resp = await client.PostAsJsonAsync("/api/v1/tutores", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostTutor_ConRutRepetido_Devuelve409()
        {
            var client = await RecepcionistaAsync();
            var dto = TestHelpers.TutorValido();
            await client.PostAsJsonAsync("/api/v1/tutores", dto);

            var resp = await client.PostAsJsonAsync("/api/v1/tutores", dto);

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PutTutor_CambiaLosDatos_Devuelve204()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);
            var dto = TestHelpers.TutorValido();
            dto.Rut = tutor.Rut;
            dto.Nombre = "Nombre Editado";

            var resp = await client.PutAsJsonAsync($"/api/v1/tutores/{tutor.IdTutor}", dto);

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
            var editado = await client.GetFromJsonAsync<TutorDto>($"/api/v1/tutores/{tutor.IdTutor}");
            Assert.Equal("Nombre Editado", editado!.Nombre);
        }

        [Fact]
        public async Task DeleteTutor_ComoRecepcionista_Devuelve403()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);

            var resp = await client.DeleteAsync($"/api/v1/tutores/{tutor.IdTutor}");

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteTutor_ConMascotas_Devuelve409()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var mascota = await TestHelpers.CrearMascotaAsync(admin);

            var resp = await admin.DeleteAsync($"/api/v1/tutores/{mascota.IdTutor}");

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostMascota_Valida_Devuelve201ConEdadYNombres()
        {
            var client = await RecepcionistaAsync();

            var mascota = await TestHelpers.CrearMascotaAsync(client);

            Assert.Equal(3, mascota.Edad);
            Assert.Equal("Perro", mascota.Especie);
            Assert.Equal("Labrador Retriever", mascota.Raza);
            Assert.Equal("María González", mascota.NombreTutor);
        }

        [Fact]
        public async Task PostMascota_ConRazaDeOtraEspecie_Devuelve400()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);
            var dto = TestHelpers.MascotaValida(tutor.IdTutor);
            dto.IdRaza = 5; // Siamés es raza de Gato, la mascota es Perro

            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostMascota_ConMicrochipRepetido_Devuelve409()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);
            var dto = TestHelpers.MascotaValida(tutor.IdTutor);
            dto.Microchip = TestHelpers.MicrochipNuevo();
            await client.PostAsJsonAsync("/api/v1/mascotas", dto);

            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", dto);

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostMascota_ConMicrochipDe10Digitos_Devuelve400()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);
            var dto = TestHelpers.MascotaValida(tutor.IdTutor);
            dto.Microchip = "1234567890";

            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostMascota_ConTutorInexistente_Devuelve400()
        {
            var client = await RecepcionistaAsync();

            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", TestHelpers.MascotaValida(999999));

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostMascota_ConFechaDeNacimientoFutura_Devuelve400()
        {
            var client = await RecepcionistaAsync();
            var tutor = await TestHelpers.CrearTutorAsync(client);
            var dto = TestHelpers.MascotaValida(tutor.IdTutor);
            dto.FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(3));

            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }
    }
}
