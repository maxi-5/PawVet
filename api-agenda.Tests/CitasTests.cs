using System.Net;
using System.Net.Http.Json;
using PawVetAgenda.DTOs;

namespace PawVetAgenda.Tests
{
    // Agendar, reprogramar y cambiar estado de citas (RF-07, RF-08, RF-12, HU-03, Secuencia 2)
    public class CitasTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public CitasTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PostCita_Valida_Devuelve201EnEstadoAgendada()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(TestHelpers.HorarioLibre()));

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var cita = await resp.Content.ReadFromJsonAsync<CitaDto>();
            Assert.Equal("Agendada", cita!.Estado);
        }

        [Fact]
        public async Task PostCita_SinToken_Devuelve401()
        {
            var resp = await _factory.CreateClient().PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(TestHelpers.HorarioLibre()));

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_ComoVeterinario_Devuelve403()
        {
            var client = _factory.CreateClient().ComoVeterinario(1);

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(TestHelpers.HorarioLibre()));

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_ConFechaPasada_Devuelve400()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(DateTime.Now.AddHours(-2)));

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_ConMascotaInexistenteEnClinica_Devuelve400()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(TestHelpers.HorarioLibre(), idMascota: 999));

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_ConApiClinicaCaida_Devuelve503()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(TestHelpers.HorarioLibre(), idMascota: FakeClinicaClient.IdCaida));

            Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
            var problema = await resp.Content.ReadAsStringAsync();
            Assert.Contains("API Clinica", problema);
        }

        [Fact]
        public async Task PostCita_EnDiaFeriado_Devuelve400()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var fecha = FakeFeriadosClient.DiaFeriado.ToDateTime(new TimeOnly(11, 0));

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(fecha));

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Contains("feriado", await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PostCita_QueSeTopaConOtraDelMismoVeterinario_Devuelve409()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var hora = TestHelpers.HorarioLibre();
            await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora));

            // empieza 15 minutos despues: se topa con la cita de 30 minutos
            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora.AddMinutes(15), idMascota: 2));

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_MismoHorarioOtroVeterinario_Devuelve201()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var hora = TestHelpers.HorarioLibre();
            await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora, idVeterinario: 1));

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora, idMascota: 2, idVeterinario: 2));

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact]
        public async Task PostCita_EnHorarioDeUnaCitaCancelada_Devuelve201()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);
            await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Cancelada" });

            var resp = await client.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(cita.FechaHora, idMascota: 2));

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact]
        public async Task PatchEstado_Confirmar_Devuelve200()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var confirmada = await TestHelpers.CrearCitaConfirmadaAsync(client);

            Assert.Equal("Confirmada", confirmada.Estado);
        }

        [Fact]
        public async Task PatchEstado_ConfirmarUnaCitaCancelada_Devuelve409()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);
            await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Cancelada" });

            var resp = await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Confirmada" });

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PatchEstado_NoAsistioAntesDeLaHora_Devuelve409()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);

            var resp = await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "NoAsistio" });

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PatchEstado_ConEstadoNoPermitido_Devuelve400()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);

            // Atendida solo se alcanza registrando la atencion
            var resp = await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Atendida" });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PutCita_Reprogramar_Devuelve204()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);

            var resp = await client.PutAsJsonAsync($"/api/v1/citas/{cita.IdCita}", TestHelpers.CitaValida(cita.FechaHora.AddHours(2)));

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        [Fact]
        public async Task PutCita_Cancelada_Devuelve409()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);
            await client.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Cancelada" });

            var resp = await client.PutAsJsonAsync($"/api/v1/citas/{cita.IdCita}", TestHelpers.CitaValida(cita.FechaHora.AddHours(2)));

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteCita_ComoRecepcionista_Devuelve403()
        {
            var client = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(client);

            var resp = await client.DeleteAsync($"/api/v1/citas/{cita.IdCita}");

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteCita_ComoAdministrador_Devuelve204()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(recepcion);
            var admin = _factory.CreateClient().ComoAdmin();

            var resp = await admin.DeleteAsync($"/api/v1/citas/{cita.IdCita}");

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        [Fact]
        public async Task GetMiAgenda_SoloDevuelveLasCitasDelVeterinarioLogueado()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var hora = TestHelpers.HorarioLibre();
            await recepcion.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora, idVeterinario: 1));
            await recepcion.PostAsJsonAsync("/api/v1/citas", TestHelpers.CitaValida(hora, idMascota: 2, idVeterinario: 2));
            var vet = _factory.CreateClient().ComoVeterinario(2);

            var lista = await vet.GetFromJsonAsync<List<CitaDto>>($"/api/v1/citas/mi-agenda?fecha={hora:yyyy-MM-dd}");

            Assert.Single(lista!);
            Assert.Equal(2, lista![0].IdVeterinario);
        }

        [Fact]
        public async Task GetMiAgenda_VeterinarioSinFicha_Devuelve403()
        {
            var client = _factory.CreateClient().ConToken(TestHelpers.Token("Veterinario"));

            var resp = await client.GetAsync("/api/v1/citas/mi-agenda");

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task GetCita_Inexistente_Devuelve404ConProblemDetails()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.GetAsync("/api/v1/citas/999999");

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType!.MediaType);
        }
    }
}
