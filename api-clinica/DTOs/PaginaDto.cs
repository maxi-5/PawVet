namespace PawVetClinica.DTOs
{
    // Respuesta paginada (listados)
    public class PaginaDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public int Total { get; set; }
    }
}
