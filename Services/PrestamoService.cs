using System;
using System.Linq;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services
{
    public class PrestamoService
    {
        private readonly BibliotecaDbContext _context;

        public PrestamoService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public ResultadoOperacion RegistrarPrestamo(int nroSocio, string isbn)
        {
            var estadoActivoId = _context.EstadoPrestamos
                .Where(e => e.Estado == "Activo")
                .Select(e => e.Id)
                .First();

            var socio = _context.Socios
                .Include(s => s.TipoSocio)
                .FirstOrDefault(s => s.NroSocio == nroSocio);

            if (socio == null)
                return new ResultadoOperacion { Mensaje = "El socio no existe." };

            if (!socio.Activo)
                return new ResultadoOperacion { Mensaje = "El socio esta inactivo." };

            // RN-02: multas pendientes = MultaGenerada > 0 y MultaPagada = false
            var tieneMultas = _context.Prestamos
                .Any(p => p.NroSocio == nroSocio
                       && p.MultaGenerada > 0
                       && !p.MultaPagada);

            if (tieneMultas)
                return new ResultadoOperacion { Mensaje = "El socio tiene multas pendientes." };

            var prestamosActivosSocio = _context.Prestamos
                .Count(p => p.NroSocio == nroSocio && p.EstadoPrestamoId == estadoActivoId);

            if (prestamosActivosSocio >= socio.TipoSocio.MaxLibrosSimultaneos)
                return new ResultadoOperacion { Mensaje = "Limite de prestamos simultaneos alcanzado." };

            var libro = _context.Libros.Find(isbn);
            if (libro == null)
                return new ResultadoOperacion { Mensaje = "El libro no existe." };

            var copiasActivas = _context.Prestamos
                .Count(p => p.ISBN == isbn && p.EstadoPrestamoId == estadoActivoId);

            var disponibles = libro.CantidadCopias - copiasActivas;

            if (disponibles <= 0)
                return new ResultadoOperacion
                {
                    Mensaje = "No hay copias disponibles del libro.",
                    OfrecerReserva = true
                };

            var fechaPrestamo = DateTime.Today;
            var fechaVencimiento = fechaPrestamo.AddDays(socio.TipoSocio.DiasPrestamo);

            var prestamo = new Prestamo
            {
                NroSocio = nroSocio,
                ISBN = isbn,
                FechaPrestamo = fechaPrestamo,
                FechaVencimiento = fechaVencimiento,
                EstadoPrestamoId = estadoActivoId,
                MultaPagada = false
            };

            _context.Prestamos.Add(prestamo);
            _context.SaveChanges();

            return new ResultadoOperacion
            {
                Exitoso = true,
                Mensaje = "Prestamo registrado. Vence: " + fechaVencimiento.ToString("yyyy-MM-dd")
            };
        }
    }
}
