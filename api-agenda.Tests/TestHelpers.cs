using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PawVetAgenda.DTOs;

namespace PawVetAgenda.Tests
{
    public static class TestHelpers
    {
        public const string LlaveJwt = "llave_de_pruebas_integracion_pawvet_2026!!!!";

        private static int _dia = 0;

        // Genera un JWT igual al que emite la API Clinica (misma llave, issuer, audience y claims)
        public static string Token(string rol, int? idVeterinario = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "prueba@pawvet.cl"),
                new Claim("id_user", "1"),
                new Claim("nombre", "Usuario de Prueba"),
                new Claim(ClaimTypes.Role, rol)
            };
            if (idVeterinario.HasValue)
                claims.Add(new Claim("id_veterinario", idVeterinario.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LlaveJwt));
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = "PawVet.ApiClinica",
                Audience = "PawVet",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public static HttpClient ConToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static HttpClient ComoAdmin(this HttpClient c) => c.ConToken(Token("Administrador"));
        public static HttpClient ComoRecepcionista(this HttpClient c) => c.ConToken(Token("Recepcionista"));
        public static HttpClient ComoVeterinario(this HttpClient c, int idVeterinario) => c.ConToken(Token("Veterinario", idVeterinario));

        // cada llamada usa un dia futuro distinto para que las pruebas no se topen entre si
        public static DateTime HorarioLibre()
        {
            var dia = Interlocked.Increment(ref _dia);
            return DateTime.Today.AddDays(dia).AddHours(10);
        }

        public static CitaGuardar CitaValida(DateTime fechaHora, int idMascota = 1, int idVeterinario = 1) => new()
        {
            IdMascota = idMascota,
            IdVeterinario = idVeterinario,
            FechaHora = fechaHora,
            DuracionMinutos = 30,
            Motivo = "Control anual"
        };

        public static async Task<CitaDto> CrearCitaAsync(HttpClient recepcionista, int idVeterinario = 1, int idMascota = 1)
        {
            var resp = await recepcionista.PostAsJsonAsync("/api/v1/citas", CitaValida(HorarioLibre(), idMascota, idVeterinario));
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<CitaDto>())!;
        }

        public static async Task<CitaDto> CrearCitaConfirmadaAsync(HttpClient recepcionista, int idVeterinario = 1, int idMascota = 1)
        {
            var cita = await CrearCitaAsync(recepcionista, idVeterinario, idMascota);
            var resp = await recepcionista.PatchAsJsonAsync($"/api/v1/citas/{cita.IdCita}/estado", new CambioEstadoCita { Estado = "Confirmada" });
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<CitaDto>())!;
        }
    }
}
