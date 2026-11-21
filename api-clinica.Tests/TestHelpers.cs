using System.Net.Http.Headers;
using System.Net.Http.Json;
using PawVetClinica.DTOs;
using PawVetClinica.Models;

namespace PawVetClinica.Tests
{
    public static class TestHelpers
    {
        public const string AdminEmail = "admin@pawvet.cl";
        public const string AdminPassword = "Admin123!";
        public const string PasswordPrueba = "Secreta123";

        private static int _contador = 10000000;

        public static async Task<LoginResponse> LoginAsync(HttpClient client, string email, string password)
        {
            var resp = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest { Email = email, Password = password });
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<LoginResponse>())!;
        }

        public static HttpClient ConToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static async Task<HttpClient> ComoAdminAsync(this HttpClient client)
        {
            var login = await LoginAsync(client, AdminEmail, AdminPassword);
            return client.ConToken(login.Token);
        }

        // el administrador crea un usuario con el rol indicado; el cliente queda logueado con ese usuario
        public static async Task<UsuarioDto> ComoNuevoUsuarioAsync(this HttpClient client, HttpClient admin, int idRol)
        {
            var dto = new UsuarioCrear { Nombre = "Usuario de Prueba", Email = $"u{Guid.NewGuid():N}@pawvet.cl", Password = PasswordPrueba, IdRol = idRol };
            var resp = await admin.PostAsJsonAsync("/api/v1/usuarios", dto);
            resp.EnsureSuccessStatusCode();
            var usuario = (await resp.Content.ReadFromJsonAsync<UsuarioDto>())!;

            var login = await LoginAsync(client, dto.Email, PasswordPrueba);
            client.ConToken(login.Token);
            return usuario;
        }

        // genera un RUT chileno valido y distinto en cada llamada
        public static string RutValido()
        {
            var cuerpo = Interlocked.Increment(ref _contador).ToString();
            int suma = 0, multiplicador = 2;
            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += (cuerpo[i] - '0') * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }
            int resto = 11 - (suma % 11);
            char dv = resto == 11 ? '0' : resto == 10 ? 'K' : (char)('0' + resto);
            return $"{cuerpo}-{dv}";
        }

        public static TutorGuardar TutorValido() => new()
        {
            Rut = RutValido(),
            Nombre = "María González",
            Telefono = "+56912345678",
            Email = "maria@correo.cl",
            Direccion = "Villagrán 455, Mulchén"
        };

        public static async Task<TutorDto> CrearTutorAsync(HttpClient client)
        {
            var resp = await client.PostAsJsonAsync("/api/v1/tutores", TutorValido());
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<TutorDto>())!;
        }

        // perro labrador (datos iniciales: especie 1, raza 1)
        public static MascotaGuardar MascotaValida(int idTutor) => new()
        {
            Nombre = "Rocky",
            Sexo = "Macho",
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-3)),
            Peso = 25.4m,
            IdTutor = idTutor,
            IdEspecie = 1,
            IdRaza = 1
        };

        public static async Task<MascotaDto> CrearMascotaAsync(HttpClient client)
        {
            var tutor = await CrearTutorAsync(client);
            var resp = await client.PostAsJsonAsync("/api/v1/mascotas", MascotaValida(tutor.IdTutor));
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<MascotaDto>())!;
        }

        // microchip de 15 digitos distinto en cada llamada
        public static string MicrochipNuevo()
        {
            return "941" + Interlocked.Increment(ref _contador).ToString().PadLeft(12, '0');
        }

        public const int RolRecepcionista = Rol.RECEPCIONISTA;
        public const int RolVeterinario = Rol.VETERINARIO;
    }
}
