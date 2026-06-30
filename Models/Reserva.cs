using System;

namespace Biblioteca.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int NroSocio { get; set; }
        public string ISBN { get; set; }
        public DateTime FechaReserva { get; set; }
        public int EstadoReservaId { get; set; }

        public Socio Socio { get; set; }
        public Libro Libro { get; set; }
        public EstadoReserva EstadoReserva { get; set; }
    }
}
