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
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public int TipoSocioId { get; set; }
        public bool Activo { get; set; }

        public TipoSocio TipoSocio { get; set; }
        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}
