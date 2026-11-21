using PawVetAgenda.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PawVetAgenda.Services
{
    // Resultado de consultar un recurso en la API Clinica
    public enum EstadoConsulta
    {
        Encontrado,
        NoEncontrado,   // la API Clinica respondio 404
        NoDisponible    // no respondio en 5 segundos o fallo
    }

    public class ResultadoConsulta<T>
    {
        public EstadoConsulta Estado { get; set; }
        public T? Dato { get; set; }

        public static ResultadoConsulta<T> Encontrado(T dato) => new() { Estado = EstadoConsulta.Encontrado, Dato = dato };
        public static ResultadoConsulta<T> NoEncontrado() => new() { Estado = EstadoConsulta.NoEncontrado };
        public static ResultadoConsulta<T> NoDisponible() => new() { Estado = EstadoConsulta.NoDisponible };
    }

    // Integracion entre servicios: la API Agenda consume la API Clinica (ADR-003, RNF-05)
    public interface IClinicaClient
    {
        Task<ResultadoConsulta<MascotaInfo>> ObtenerMascotaAsync(int idMascota, string token);
        Task<ResultadoConsulta<VeterinarioInfo>> ObtenerVeterinarioAsync(int idVeterinario, string token);
    }

    // Cliente HTTP tipado creado por IHttpClientFactory (ver Program.cs: BaseAddress y Timeout de 5 s)
    public class ClinicaClient : IClinicaClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ClinicaClient> _logger;

        public ClinicaClient(HttpClient http, ILogger<ClinicaClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public Task<ResultadoConsulta<MascotaInfo>> ObtenerMascotaAsync(int idMascota, string token)
        {
            return ConsultarAsync<MascotaInfo>($"api/v1/mascotas/{idMascota}", token);
        }

        public Task<ResultadoConsulta<VeterinarioInfo>> ObtenerVeterinarioAsync(int idVeterinario, string token)
        {
            return ConsultarAsync<VeterinarioInfo>($"api/v1/veterinarios/{idVeterinario}", token);
        }

        private async Task<ResultadoConsulta<T>> ConsultarAsync<T>(string ruta, string token)
        {
            try
            {
                var solicitud = new HttpRequestMessage(HttpMethod.Get, ruta);
                // se reenvia el token del usuario que hizo la peticion (ADR-002)
                solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await _http.SendAsync(solicitud);

                if (respuesta.StatusCode == HttpStatusCode.NotFound)
                    return ResultadoConsulta<T>.NoEncontrado();

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("La API Clinica respondio {Codigo} en {Ruta}", (int)respuesta.StatusCode, ruta);
                    return ResultadoConsulta<T>.NoDisponible();
                }

                var dato = await respuesta.Content.ReadFromJsonAsync<T>();
                return dato == null ? ResultadoConsulta<T>.NoDisponible() : ResultadoConsulta<T>.Encontrado(dato);
            }
            catch (HttpRequestException ex)
            {
                // la API Clinica esta caida o no se pudo conectar
                _logger.LogWarning("No se pudo conectar con la API Clinica: {Mensaje}", ex.Message);
                return ResultadoConsulta<T>.NoDisponible();
            }
            catch (TaskCanceledException)
            {
                // se cumplio el tiempo de espera de 5 segundos
                _logger.LogWarning("La API Clinica no respondio dentro del tiempo de espera ({Ruta})", ruta);
                return ResultadoConsulta<T>.NoDisponible();
            }
        }
    }
}
