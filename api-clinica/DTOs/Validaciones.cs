using System.ComponentModel.DataAnnotations;

namespace PawVetClinica.DTOs
{
    // Valida un RUT chileno con formato 12345678-9 (sin puntos) y digito verificador correcto
    public class RutChilenoAttribute : ValidationAttribute
    {
        public RutChilenoAttribute()
        {
            ErrorMessage = "El RUT debe tener el formato 12345678-9 (sin puntos) y un digito verificador valido";
        }

        public override bool IsValid(object? value)
        {
            if (value is not string rut || string.IsNullOrWhiteSpace(rut))
                return true; // [Required] se encarga del campo vacio

            return EsValido(rut);
        }

        public static bool EsValido(string rut)
        {
            rut = rut.Trim().ToUpper();
            var partes = rut.Split('-');
            if (partes.Length != 2 || partes[0].Length < 7 || partes[0].Length > 8 || partes[1].Length != 1)
                return false;
            if (!partes[0].All(char.IsDigit))
                return false;

            // calculo del digito verificador (modulo 11)
            int suma = 0, multiplicador = 2;
            for (int i = partes[0].Length - 1; i >= 0; i--)
            {
                suma += (partes[0][i] - '0') * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }
            int resto = 11 - (suma % 11);
            char esperado = resto == 11 ? '0' : resto == 10 ? 'K' : (char)('0' + resto);

            return partes[1][0] == esperado;
        }
    }
}
