using System.Collections.Generic;

namespace Biblioteca.Models
{
    public class TipoSocio
    {
        public int Id { get; set; }
        public string Clase { get; set; } = null!;
        public int MaxLibrosSimultaneos { get; set; }
        public int DiasPrestamo { get; set; }
        public decimal MultaPorDia { get; set; }

        public ICollection<Socio> Socios { get; set; } = new List<Socio>();
    }
}
