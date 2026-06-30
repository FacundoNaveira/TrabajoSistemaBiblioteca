using System;

namespace Biblioteca.Models
{
    public class Prestamo
    {
        public int Id { get; set; }
        public int NroSocio { get; set; }
        public string ISBN { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public int EstadoPrestamoId { get; set; }
        public decimal? MultaGenerada { get; set; }

        public Socio Socio { get; set; }
        public Libro Libro { get; set; }
        public EstadoPrestamo EstadoPrestamo { get; set; }
    }
}
