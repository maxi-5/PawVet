using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using PawVetClinica.DTOs;

namespace PawVetClinica.Tests
{
    // Inicio de sesion (RF-01, HU-01) y usuarios (RF-02, HU-06)
    public class AuthYUsuariosTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthYUsuariosTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_ConCredencialesCorrectas_DevuelveTokenConRol()
        {
            var client = _factory.CreateClient();

            var login = await TestHelpers.LoginAsync(client, TestHelpers.AdminEmail, TestHelpers.AdminPassword);

            Assert.Equal("Administrador", login.Usuario.Rol);
            Assert.True(login.Expira > DateTime.UtcNow.AddMinutes(55));
        }

        [Fact]
        public async Task Login_ConPasswordIncorrecta_Devuelve401()
        {
            var client = _factory.CreateClient();

            var resp = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest { Email = TestHelpers.AdminEmail, Password = "incorrecta" });

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task Login_ConEmailSinFormato_Devuelve400()
        {
            var client = _factory.CreateClient();

            var resp = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest { Email = "no-es-email", Password = "x" });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Login_DeVeterinarioVinculado_IncluyeIdVeterinarioEnElToken()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var vetClient = _factory.CreateClient();
            var usuario = await vetClient.ComoNuevoUsuarioAsync(admin, TestHelpers.RolVeterinario);
            var ficha = await admin.PostAsJsonAsync("/api/v1/veterinarios", new VeterinarioGuardar { Rut = TestHelpers.RutValido(), Nombre = "Camila Soto", IdUser = usuario.IdUser });
            var vet = await ficha.Content.ReadFromJsonAsync<VeterinarioDto>();

            var login = await TestHelpers.LoginAsync(_factory.CreateClient(), usuario.Email, TestHelpers.PasswordPrueba);

            Assert.Equal(vet!.IdVeterinario, login.IdVeterinario);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
            Assert.Equal(vet.IdVeterinario.ToString(), jwt.Claims.First(c => c.Type == "id_veterinario").Value);
        }

        [Fact]
        public async Task GetUsuarios_SinToken_Devuelve401()
        {
            var resp = await _factory.CreateClient().GetAsync("/api/v1/usuarios");

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task GetUsuarios_ComoRecepcionista_Devuelve403()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var client = _factory.CreateClient();
            await client.ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);

            var resp = await client.GetAsync("/api/v1/usuarios");

            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task PostUsuario_ConEmailRepetido_Devuelve409()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var dto = new UsuarioCrear { Nombre = "Diego Fuentes", Email = $"d{Guid.NewGuid():N}@pawvet.cl", Password = "Secreta123", IdRol = 2 };
            await admin.PostAsJsonAsync("/api/v1/usuarios", dto);

            var resp = await admin.PostAsJsonAsync("/api/v1/usuarios", dto);

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task PostUsuario_ConRolInexistente_Devuelve400()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();

            var resp = await admin.PostAsJsonAsync("/api/v1/usuarios",
                new UsuarioCrear { Nombre = "Diego Fuentes", Email = $"d{Guid.NewGuid():N}@pawvet.cl", Password = "Secreta123", IdRol = 99 });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteUsuario_ElPropioAdministrador_Devuelve409()
        {
            var client = _factory.CreateClient();
            var login = await TestHelpers.LoginAsync(client, TestHelpers.AdminEmail, TestHelpers.AdminPassword);
            client.ConToken(login.Token);

            var resp = await client.DeleteAsync($"/api/v1/usuarios/{login.Usuario.IdUser}");

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteUsuario_Existente_Devuelve204YLuego404()
        {
            var admin = await _factory.CreateClient().ComoAdminAsync();
            var usuario = await _factory.CreateClient().ComoNuevoUsuarioAsync(admin, TestHelpers.RolRecepcionista);

            var resp = await admin.DeleteAsync($"/api/v1/usuarios/{usuario.IdUser}");
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            var get = await admin.GetAsync($"/api/v1/usuarios/{usuario.IdUser}");
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }
    }
}
