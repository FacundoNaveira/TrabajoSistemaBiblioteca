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
            var resultado = await _context.Prestamos
                .Where(p => p.MultaGenerada != null && p.MultaPagada == false)
                .GroupBy(p => new { p.NroSocio, p.Socio.Nombre, p.Socio.Apellido })
                .Select(g => new {
                    g.Key.NroSocio,
                    g.Key.Nombre,
                    g.Key.Apellido,
                    TotalMultas = g.Sum(p => p.MultaGenerada.Value)
                })
                .OrderByDescending(x => x.TotalMultas)
                .ToListAsync();

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
    }
}
