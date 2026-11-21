using System.Net;
using System.Net.Http.Json;
using PawVetAgenda.DTOs;

namespace PawVetAgenda.Tests
{
    // Atenciones (RF-10, HU-05) y vacunaciones (RF-11, RF-13)
    public class AtencionesYVacunasTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AtencionesYVacunasTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static AtencionCrear Atencion(int idCita) => new()
        {
            IdCita = idCita,
            Diagnostico = "Paciente sano, control sin observaciones",
            Tratamiento = "Desparasitación",
            Peso = 25.4m,
            Temperatura = 38.5m
        };

        [Fact]
        public async Task PostAtencion_SobreCitaConfirmada_Devuelve201YLaCitaQuedaAtendida()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion, idVeterinario: 1);
            var vet = _factory.CreateClient().ComoVeterinario(1);

            var resp = await vet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var actualizada = await recepcion.GetFromJsonAsync<CitaDto>($"/api/v1/citas/{cita.IdCita}");
            Assert.Equal("Atendida", actualizada!.Estado);
        }

        [Fact]
        public async Task PostAtencion_SobreCitaAgendada_Devuelve409()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaAsync(recepcion, idVeterinario: 1);
            var vet = _factory.CreateClient().ComoVeterinario(1);

            var resp = await vet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostAtencion_DeCitaDeOtroVeterinario_Devuelve403()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion, idVeterinario: 1);
            var otroVet = _factory.CreateClient().ComoVeterinario(2);

            var resp = await otroVet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostAtencion_DosVecesLaMismaCita_Devuelve409()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion, idVeterinario: 1);
            var vet = _factory.CreateClient().ComoVeterinario(1);
            await vet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            var resp = await vet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostAtencion_ComoRecepcionista_Devuelve403()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion);

            var resp = await recepcion.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostAtencion_ConTemperaturaFueraDeRango_Devuelve400()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion, idVeterinario: 1);
            var vet = _factory.CreateClient().ComoVeterinario(1);
            var dto = Atencion(cita.IdCita);
            dto.Temperatura = 60m;

            var resp = await vet.PostAsJsonAsync("/api/v1/atenciones", dto);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetAtenciones_PorMascota_DevuelveElHistorial()
        {
            var recepcion = _factory.CreateClient().ComoRecepcionista();
            var cita = await TestHelpers.CrearCitaConfirmadaAsync(recepcion, idVeterinario: 1, idMascota: 2);
            var vet = _factory.CreateClient().ComoVeterinario(1);
            await vet.PostAsJsonAsync("/api/v1/atenciones", Atencion(cita.IdCita));

            var lista = await vet.GetFromJsonAsync<List<AtencionDto>>("/api/v1/atenciones?idMascota=2");

            Assert.NotEmpty(lista!);
            Assert.All(lista!, a => Assert.Equal(2, a.IdMascota));
        }

        [Fact]
        public async Task PostVacunacion_Valida_CalculaLaProximaDosis()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // vacuna 1 = Antirrábica canina (365 dias), mascota 1 = perro
            var resp = await vet.PostAsJsonAsync("/api/v1/vacunaciones", new VacunacionCrear { IdMascota = 1, IdVacuna = 1, FechaAplicacion = hoy, Lote = "L-2026-01" });

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var vacunacion = await resp.Content.ReadFromJsonAsync<VacunacionDto>();
            Assert.Equal(hoy.AddDays(365), vacunacion!.FechaProxima);
        }

        [Fact]
        public async Task PostVacunacion_VacunaDeOtraEspecie_Devuelve400()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);

            // vacuna 4 = Antirrábica felina, mascota 1 = perro
            var resp = await vet.PostAsJsonAsync("/api/v1/vacunaciones",
                new VacunacionCrear { IdMascota = 1, IdVacuna = 4, FechaAplicacion = DateOnly.FromDateTime(DateTime.Today) });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostVacunacion_ConFechaFutura_Devuelve400()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);

            var resp = await vet.PostAsJsonAsync("/api/v1/vacunaciones",
                new VacunacionCrear { IdMascota = 1, IdVacuna = 1, FechaAplicacion = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PostVacunacion_ConApiClinicaCaida_Devuelve503()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);

            var resp = await vet.PostAsJsonAsync("/api/v1/vacunaciones",
                new VacunacionCrear { IdMascota = FakeClinicaClient.IdCaida, IdVacuna = 1, FechaAplicacion = DateOnly.FromDateTime(DateTime.Today) });

            Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        }

        [Fact]
        public async Task PutVacunacion_CambiaLaFecha_RecalculaLaProximaDosis()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);
            var resp = await vet.PostAsJsonAsync("/api/v1/vacunaciones",
                new VacunacionCrear { IdMascota = 1, IdVacuna = 1, FechaAplicacion = DateOnly.FromDateTime(DateTime.Today) });
            var creada = await resp.Content.ReadFromJsonAsync<VacunacionDto>();
            var nuevaFecha = DateOnly.FromDateTime(DateTime.Today.AddDays(-350));

            var put = await vet.PutAsJsonAsync($"/api/v1/vacunaciones/{creada!.IdVacunacion}", new VacunacionEditar { FechaAplicacion = nuevaFecha, Lote = "L-CORREGIDO" });

            Assert.Equal(HttpStatusCode.OK, put.StatusCode);
            var editada = await put.Content.ReadFromJsonAsync<VacunacionDto>();
            Assert.Equal(nuevaFecha.AddDays(365), editada!.FechaProxima);
            Assert.Equal("L-CORREGIDO", editada.Lote);
        }

        [Fact]
        public async Task PutVacunacion_ComoRecepcionista_Devuelve403()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.PutAsJsonAsync("/api/v1/vacunaciones/1", new VacunacionEditar { FechaAplicacion = DateOnly.FromDateTime(DateTime.Today) });

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PutVacunacion_Inexistente_Devuelve404()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);

            var resp = await vet.PutAsJsonAsync("/api/v1/vacunaciones/999999", new VacunacionEditar { FechaAplicacion = DateOnly.FromDateTime(DateTime.Today) });

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetVacunaciones_SinIdMascota_Devuelve400()
        {
            var client = _factory.CreateClient().ComoRecepcionista();

            var resp = await client.GetAsync("/api/v1/vacunaciones");

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetPorVencer_IncluyeVacunaQueVenceEnLosProximosDias()
        {
            var vet = _factory.CreateClient().ComoVeterinario(1);
            // aplicada hace 350 dias -> la proxima dosis vence en 15 dias
            var aplicada = DateOnly.FromDateTime(DateTime.Today.AddDays(-350));
            await vet.PostAsJsonAsync("/api/v1/vacunaciones", new VacunacionCrear { IdMascota = 2, IdVacuna = 5, FechaAplicacion = aplicada });

            var lista = await vet.GetFromJsonAsync<List<VacunacionDto>>("/api/v1/vacunaciones/por-vencer?dias=30");

            Assert.Contains(lista!, v => v.IdMascota == 2 && v.FechaProxima == aplicada.AddDays(365));
        }
    }
}
