namespace Biblioteca.Models
{
    public class ResultadoOperacion
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = null!;
        public bool OfrecerReserva { get; set; }
    }
}
