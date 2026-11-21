using System.Security.Claims;

namespace PawVetAgenda.Services
{
    public static class ClaimsExtensions
    {
        // id del veterinario que viene en el token (solo para veterinarios con ficha vinculada)
        public static int? IdVeterinario(this ClaimsPrincipal user)
        {
            var valor = user.FindFirst("id_veterinario")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
