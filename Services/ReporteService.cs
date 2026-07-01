using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;

namespace Biblioteca.Services
{
    public class ReporteService
    {
        private readonly BibliotecaDbContext _context;

        public ReporteService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task LibrosMasPrestados()
        {
            var resultado = await _context.Prestamos
                .GroupBy(p => new { p.ISBN, p.Libro.Titulo, p.Libro.Autor })
                .Select(g => new {
                    g.Key.ISBN,
                    g.Key.Titulo,
                    g.Key.Autor,
                    CantidadPrestamos = g.Count()
                })
                .OrderByDescending(x => x.CantidadPrestamos)
                .Take(5)
                .ToListAsync();

            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine(" TOP 5 — LIBROS MÁS PRESTADOS");
            Console.WriteLine("══════════════════════════════════════════");
            
            int i = 1;
            foreach (var r in resultado)
            {
                Console.WriteLine($" #{i,-2} {r.Titulo,-20} — {r.Autor,-15} ({r.CantidadPrestamos} préstamos)");
                i++;
            }
            Console.WriteLine("══════════════════════════════════════════");
        }

        public async Task SociosConMultasPendientes()
        {
            var prestamosConMultas = await _context.Prestamos
                .Include(p => p.Socio)
                .Where(p => p.MultaGenerada != null && p.MultaPagada == false)
                .ToListAsync();

            var resultado = prestamosConMultas
                .GroupBy(p => new { p.NroSocio, p.Socio.Nombre, p.Socio.Apellido })
                .Select(g => new {
                    g.Key.NroSocio,
                    g.Key.Nombre,
                    g.Key.Apellido,
                    TotalMultas = g.Sum(p => p.MultaGenerada.Value)
                })
                .OrderByDescending(x => x.TotalMultas)
                .ToList();

            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine(" SOCIOS CON MULTAS PENDIENTES");
            Console.WriteLine("══════════════════════════════════════════");
            
            decimal totalGeneral = 0;
            foreach (var r in resultado)
            {
                Console.WriteLine($" {r.Nombre} {r.Apellido,-15} (NroSocio: {r.NroSocio,-2})   Total: ${r.TotalMultas:F2}");
                totalGeneral += r.TotalMultas;
            }
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine($" Total general: ${totalGeneral:F2}");
        }

        public async Task PrestamosVencidos()
        {
            var hoy = DateTime.Today;

            var prestamosFiltrados = await _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .Where(p => p.FechaVencimiento < hoy && p.FechaDevolucion == null)
                .ToListAsync();

            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine(" PRÉSTAMOS VENCIDOS");
            Console.WriteLine("══════════════════════════════════════════");
            
            foreach (var prestamo in prestamosFiltrados)
            {
                int diasAtraso = (hoy - prestamo.FechaVencimiento).Days;
                Console.WriteLine($" {prestamo.Socio.Nombre} {prestamo.Socio.Apellido,-15} (NroSocio: {prestamo.NroSocio})");
                Console.WriteLine($" Libro: {prestamo.Libro.Titulo} — vencido hace {diasAtraso} días (desde {prestamo.FechaVencimiento:dd/MM/yyyy})");
                Console.WriteLine();
            }
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine($" Total vencidos: {prestamosFiltrados.Count}");
        }

        public async Task DisponibilidadLibro(string busqueda)
        {
            var libro = await _context.Libros
                .Where(l => l.ISBN == busqueda || l.Titulo.Contains(busqueda))
                .Select(l => new {
                    l.ISBN,
                    l.Titulo,
                    l.Autor,
                    l.CantidadCopias,
                    PrestamosActivos = l.Prestamos
                        .Count(p => p.EstadoPrestamo.Estado == "Activo"),
                    ReservasPendientes = l.Reservas
                        .Count(r => r.EstadoReserva.Descripcion == "Pendiente")
                })
                .FirstOrDefaultAsync();

            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine(" DISPONIBILIDAD DEL LIBRO");
            Console.WriteLine("══════════════════════════════════════════");
            
            if (libro == null)
            {
                Console.WriteLine(" No se encontraron libros que coincidan con la búsqueda.");
            }
            else
            {
                Console.WriteLine($" Título:  {libro.Titulo}");
                Console.WriteLine($" Autor:   {libro.Autor}");
                Console.WriteLine($" ISBN:    {libro.ISBN}");
                Console.WriteLine($" Copias totales:      {libro.CantidadCopias}");
                Console.WriteLine($" Copias disponibles:  {libro.CantidadCopias - libro.PrestamosActivos}");
                Console.WriteLine($" Reservas pendientes: {libro.ReservasPendientes}");
            }
            Console.WriteLine("══════════════════════════════════════════");
        }

        public async Task HistorialSocio(int nroSocio)
        {
            var socio = await _context.Socios
                .Include(s => s.TipoSocio)
                .Include(s => s.Prestamos)
                    .ThenInclude(p => p.Libro)
                .Include(s => s.Prestamos)
                    .ThenInclude(p => p.EstadoPrestamo)
                .Include(s => s.Reservas)
                    .ThenInclude(r => r.Libro)
                .Include(s => s.Reservas)
                    .ThenInclude(r => r.EstadoReserva)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.NroSocio == nroSocio);

            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine(" HISTORIAL DEL SOCIO");
            
            if (socio == null)
            {
                Console.WriteLine(" Socio no encontrado.");
                Console.WriteLine("══════════════════════════════════════════");
                return;
            }

            string estadoSocio = socio.Activo ? "Activo" : "Inactivo";
            Console.WriteLine($" {socio.Nombre} {socio.Apellido} | {socio.TipoSocio.Clase} | {estadoSocio}");
            Console.WriteLine("══════════════════════════════════════════");
            
            Console.WriteLine(" PRÉSTAMOS");
            if (!socio.Prestamos.Any())
            {
                Console.WriteLine("   (Sin préstamos)");
            }
            else
            {
                foreach (var p in socio.Prestamos)
                {
                    Console.WriteLine($"  [#{p.Id}] {p.Libro.Titulo}");
                    Console.WriteLine($"       Prestado: {p.FechaPrestamo:dd/MM/yyyy} | Vence: {p.FechaVencimiento:dd/MM/yyyy}");
                    
                    string estado = p.EstadoPrestamo.Estado;
                    string icono = estado == "Vencido" ? "⚠" : (estado == "Activo" ? "✓" : "");
                    string multa = p.MultaGenerada.HasValue ? $" | Multa: ${p.MultaGenerada.Value:F2}" : "";
                    
                    Console.WriteLine($"       Estado: {estado} {icono}{multa}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine(" RESERVAS");
            if (!socio.Reservas.Any())
            {
                Console.WriteLine("   (Sin reservas)");
            }
            else
            {
                foreach (var r in socio.Reservas)
                {
                    Console.WriteLine($"  {r.Libro.Titulo} — {r.EstadoReserva.Descripcion} (desde {r.FechaReserva:dd/MM/yyyy})");
                }
            }
            Console.WriteLine("══════════════════════════════════════════");
        }
    }
}
