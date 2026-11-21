using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace PawVetAgenda.Services
{
    // Consulta los feriados de Chile en la API publica Nager.Date (RF-12)
    public interface IFeriadosClient
    {
        // devuelve el nombre del feriado, o null si el dia no es feriado (o si la API no respondio)
        Task<string?> ObtenerFeriadoAsync(DateOnly fecha);
    }

    // Cliente HTTP tipado: timeout 3 s y cache de 24 h. Si falla, se continua y se registra en el log (ADR-003).
    public class FeriadosClient : IFeriadosClient
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FeriadosClient> _logger;

        public FeriadosClient(HttpClient http, IMemoryCache cache, ILogger<FeriadosClient> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string?> ObtenerFeriadoAsync(DateOnly fecha)
        {
            var feriados = await ObtenerFeriadosDelAnioAsync(fecha.Year);
            return feriados != null && feriados.TryGetValue(fecha, out var nombre) ? nombre : null;
        }

        private async Task<Dictionary<DateOnly, string>?> ObtenerFeriadosDelAnioAsync(int anio)
        {
            var clave = $"feriados-CL-{anio}";
            if (_cache.TryGetValue(clave, out Dictionary<DateOnly, string>? enCache))
                return enCache;

            try
            {
                var lista = await _http.GetFromJsonAsync<List<FeriadoNager>>($"api/v3/PublicHolidays/{anio}/CL");
                var feriados = new Dictionary<DateOnly, string>();
                foreach (var f in lista ?? new List<FeriadoNager>())
                {
                    // solo los feriados nacionales (los regionales traen "counties")
                    if (f.Global && DateOnly.TryParse(f.Date, out var dia))
                        feriados[dia] = f.LocalName;
                }

                _cache.Set(clave, feriados, TimeSpan.FromHours(24));
                return feriados;
            }
            catch (Exception ex)
            {
                // la API de feriados no es critica: se agenda igual y queda registrado
                _logger.LogWarning("No se pudo consultar la API de feriados, se continua sin validar: {Mensaje}", ex.Message);
                return null;
            }
        }

        // formato de respuesta de Nager.Date
        private class FeriadoNager
        {
            public string Date { get; set; } = "";
            public string LocalName { get; set; } = "";
            public bool Global { get; set; }
        }
    }
}
