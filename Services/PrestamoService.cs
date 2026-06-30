using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ResultadoOperacion> RegistrarDevolucion(int idPrestamo)
        {
            var prestamo = _context.Prestamos
                .Include(p => p.Socio.TipoSocio)
                .Include(p => p.Libro)
                .FirstOrDefault(p => p.Id == idPrestamo);

            if (prestamo == null)
                return new ResultadoOperacion { Mensaje = "El prestamo no existe." };

            if (prestamo.FechaDevolucion != null)
                return new ResultadoOperacion { Mensaje = "El prestamo ya fue devuelto." };

            var estadoDevueltoId = _context.EstadoPrestamos
                .Where(e => e.Estado == "Devuelto")
                .Select(e => e.Id)
                .First();

            var fechaDevolucion = DateTime.Today;
            prestamo.FechaDevolucion = fechaDevolucion;
            prestamo.EstadoPrestamoId = estadoDevueltoId;

            var mensaje = "Devolucion registrada en termino.";
            if (fechaDevolucion > prestamo.FechaVencimiento)
            {
                var diasAtraso = (fechaDevolucion - prestamo.FechaVencimiento).Days;
                var multa = diasAtraso * prestamo.Socio.TipoSocio.MultaPorDia;
                prestamo.MultaGenerada = multa;
                mensaje = "Devolucion registrada. Dias de atraso: " + diasAtraso
                        + ". Multa generada: $" + multa.ToString("F2")
                        + " (pendiente de pago)";
            }

            await _context.SaveChangesAsync();
            await ProcesarReservas(prestamo.ISBN);

            return new ResultadoOperacion
            {
                Exitoso = true,
                Mensaje = mensaje
            };
        }

        private async Task ProcesarReservas(string isbn)
        {
            var reservaService = new ReservaService(_context);
            await reservaService.ProcesarReservasPendientes(isbn);
        }
    }
}
