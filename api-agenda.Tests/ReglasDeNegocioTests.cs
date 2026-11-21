using PawVetAgenda.Models;

namespace PawVetAgenda.Tests
{
    // Pruebas unitarias de los metodos del diagrama de clases (diagrama de estados de la Cita)
    public class ReglasDeNegocioTests
    {
        private static Cita NuevaCita(DateTime fechaHora, EstadoCita estado = EstadoCita.Agendada) => new()
        {
            FechaHora = fechaHora,
            DuracionMinutos = 30,
            Motivo = "Control",
            Estado = estado,
            IdMascota = 1,
            IdVeterinario = 1
        };

        [Fact]
        public void Confirmar_CitaAgendadaFutura_PasaAConfirmada()
        {
            var cita = NuevaCita(DateTime.Now.AddDays(1));

            cita.Confirmar();

            Assert.Equal(EstadoCita.Confirmada, cita.Estado);
        }

        [Fact]
        public void MarcarAtendida_CitaAgendada_LanzaTransicionInvalida()
        {
            var cita = NuevaCita(DateTime.Now.AddDays(1));

            Assert.Throws<TransicionInvalidaException>(() => cita.MarcarAtendida());
        }

        [Fact]
        public void MarcarNoAsistio_DespuesDeLaHora_PasaANoAsistio()
        {
            var cita = NuevaCita(DateTime.Now.AddHours(-1), EstadoCita.Confirmada);

            cita.MarcarNoAsistio();

            Assert.Equal(EstadoCita.NoAsistio, cita.Estado);
        }

        [Fact]
        public void Cancelar_CitaAtendida_LanzaTransicionInvalida()
        {
            var cita = NuevaCita(DateTime.Now.AddDays(1), EstadoCita.Atendida);

            Assert.Throws<TransicionInvalidaException>(() => cita.Cancelar());
        }

        [Theory]
        [InlineData(0, true)]     // misma hora
        [InlineData(15, true)]    // empieza a mitad de la cita
        [InlineData(30, false)]   // empieza justo cuando termina la otra
        [InlineData(-30, false)]  // termina justo cuando empieza la otra
        [InlineData(-20, true)]   // termina a mitad de la cita
        public void SeTopaCon_DetectaSolapamientoDeHorarios(int minutosDesplazamiento, bool seTopa)
        {
            var inicio = DateTime.Today.AddDays(1).AddHours(10);
            var cita = NuevaCita(inicio);

            Assert.Equal(seTopa, cita.SeTopaCon(inicio.AddMinutes(minutosDesplazamiento), 30));
        }

        [Fact]
        public void SeTopaCon_CitaCancelada_NoBloqueaElHorario()
        {
            var inicio = DateTime.Today.AddDays(1).AddHours(10);
            var cita = NuevaCita(inicio, EstadoCita.Cancelada);

            Assert.False(cita.SeTopaCon(inicio, 30));
        }

        [Fact]
        public void CalcularProximaDosis_SumaElIntervaloDeLaVacuna()
        {
            var vacunacion = new Vacunacion { FechaAplicacion = new DateOnly(2026, 1, 10) };

            var proxima = vacunacion.CalcularProximaDosis(new Vacuna { Nombre = "Antirrábica canina", IntervaloDias = 365 });

            Assert.Equal(new DateOnly(2027, 1, 10), proxima);
        }

        [Fact]
        public void EstaPorVencer_DentroDelPlazo_DevuelveTrue()
        {
            var vacunacion = new Vacunacion { FechaProxima = DateOnly.FromDateTime(DateTime.Today.AddDays(10)) };

            Assert.True(vacunacion.EstaPorVencer(30));
            Assert.False(vacunacion.EstaPorVencer(5));
        }
    }
}
