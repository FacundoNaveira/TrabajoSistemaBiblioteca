using System;

namespace Biblioteca.Models
{
    public class Prestamo
    {
        public int Id { get; set; }
        public int NroSocio { get; set; }
        public string ISBN { get; set; } = null!;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public int EstadoPrestamoId { get; set; }
        public decimal? MultaGenerada { get; set; }
        public bool MultaPagada { get; set; }

        public Socio Socio { get; set; } = null!;
        public Libro Libro { get; set; } = null!;
        public EstadoPrestamo EstadoPrestamo { get; set; } = null!;
    }
}
