using PawVetAgenda.DTOs;
using PawVetAgenda.Services;

namespace PawVetAgenda.Tests
{
    // Simula la API Clinica: asi las pruebas no dependen de que la otra API este corriendo.
    //   mascota 1 = perro (especie 1), mascota 2 = gato (especie 2)
    //   veterinarios 1 y 2
    //   id 500 = la API Clinica "no responde" (simula el timeout -> 503)
    //   cualquier otro id = no existe (404 en la API Clinica)
    public class FakeClinicaClient : IClinicaClient
    {
        public const int IdCaida = 500;

        public Task<ResultadoConsulta<MascotaInfo>> ObtenerMascotaAsync(int idMascota, string token)
        {
            if (idMascota == IdCaida) return Task.FromResult(ResultadoConsulta<MascotaInfo>.NoDisponible());
            if (idMascota == 1) return Task.FromResult(ResultadoConsulta<MascotaInfo>.Encontrado(new MascotaInfo { IdMascota = 1, Nombre = "Rocky", IdEspecie = 1 }));
            if (idMascota == 2) return Task.FromResult(ResultadoConsulta<MascotaInfo>.Encontrado(new MascotaInfo { IdMascota = 2, Nombre = "Michi", IdEspecie = 2 }));
            return Task.FromResult(ResultadoConsulta<MascotaInfo>.NoEncontrado());
        }

        public Task<ResultadoConsulta<VeterinarioInfo>> ObtenerVeterinarioAsync(int idVeterinario, string token)
        {
            if (idVeterinario == IdCaida) return Task.FromResult(ResultadoConsulta<VeterinarioInfo>.NoDisponible());
            if (idVeterinario == 1 || idVeterinario == 2)
                return Task.FromResult(ResultadoConsulta<VeterinarioInfo>.Encontrado(new VeterinarioInfo { IdVeterinario = idVeterinario, Nombre = $"Veterinario {idVeterinario}" }));
            return Task.FromResult(ResultadoConsulta<VeterinarioInfo>.NoEncontrado());
        }
    }

    // Simula la API de feriados: solo DiaFeriado es feriado
    public class FakeFeriadosClient : IFeriadosClient
    {
        public static readonly DateOnly DiaFeriado = DateOnly.FromDateTime(DateTime.Today.AddDays(300));

        public Task<string?> ObtenerFeriadoAsync(DateOnly fecha)
        {
            return Task.FromResult(fecha == DiaFeriado ? "Feriado de prueba" : null);
        }
    }
}
