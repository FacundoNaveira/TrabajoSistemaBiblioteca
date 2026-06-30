using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Libro
    {
        [Key]
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Genero { get; set; }
        public int CantidadCopias { get; set; }

        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}
