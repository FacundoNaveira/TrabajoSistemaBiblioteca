using System.Collections.Generic;

namespace Biblioteca.Models
{
    public class EstadoReserva
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }

        public ICollection<Reserva> Reservas { get; set; }
    }
}
