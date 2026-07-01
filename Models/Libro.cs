using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Libro
    {
        [Key]
        public string ISBN { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Autor { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public int CantidadCopias { get; set; }

        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
