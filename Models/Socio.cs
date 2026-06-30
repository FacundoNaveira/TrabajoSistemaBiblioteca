using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Models
{
    public class Socio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NroSocio { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int TipoSocioId { get; set; }
        public bool Activo { get; set; }

        public TipoSocio TipoSocio { get; set; } = null!;
        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
