using System.Collections.Generic;

namespace Biblioteca.Models
{
    public class EstadoPrestamo
    {
        public int Id { get; set; }
        public string Estado { get; set; } = null!;

        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}
