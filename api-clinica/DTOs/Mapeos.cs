using PawVetClinica.Models;

namespace PawVetClinica.DTOs
{
    // Convierte las entidades en los DTOs de salida (en un solo lugar)
    public static class Mapeos
    {
        public static UsuarioDto ToDto(this Usuario u) => new()
        {
            IdUser = u.IdUser,
            Nombre = u.Nombre,
            Email = u.Email,
            IdRol = u.IdRol,
            Rol = u.Rol?.Nombre ?? ""
        };

        public static TutorDto ToDto(this Tutor t) => new()
        {
            IdTutor = t.IdTutor,
            Rut = t.Rut,
            Nombre = t.Nombre,
            Telefono = t.Telefono,
            Email = t.Email,
            Direccion = t.Direccion,
            CantidadMascotas = t.Mascotas.Count
        };

        public static MascotaDto ToDto(this Mascota m) => new()
        {
            IdMascota = m.IdMascota,
            Nombre = m.Nombre,
            Sexo = m.Sexo,
            FechaNacimiento = m.FechaNacimiento,
            Edad = m.CalcularEdad(),
            Peso = m.Peso,
            Microchip = m.Microchip,
            IdTutor = m.IdTutor,
            NombreTutor = m.Tutor?.Nombre,
            IdEspecie = m.IdEspecie,
            Especie = m.Especie?.Nombre,
            IdRaza = m.IdRaza,
            Raza = m.Raza?.Nombre
        };

        public static VeterinarioDto ToDto(this Veterinario v) => new()
        {
            IdVeterinario = v.IdVeterinario,
            Rut = v.Rut,
            Nombre = v.Nombre,
            Especialidad = v.Especialidad,
            Telefono = v.Telefono,
            Email = v.Email,
            IdUser = v.IdUser
        };

        public static EspecieDto ToDto(this Especie e) => new()
        {
            IdEspecie = e.IdEspecie,
            Nombre = e.Nombre
        };

        public static RazaDto ToDto(this Raza r) => new()
        {
            IdRaza = r.IdRaza,
            Nombre = r.Nombre,
            IdEspecie = r.IdEspecie,
            Especie = r.Especie?.Nombre
        };
    }
}
