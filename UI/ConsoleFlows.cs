using System;
using System.Linq;
using System.Collections.Generic;
using Biblioteca.Data;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.UI
{
    public class ConsoleFlows
    {
        private readonly BibliotecaDbContext _context;
        private readonly PrestamoService _prestamoService;
        private readonly ReservaService _reservaService;

        public ConsoleFlows(BibliotecaDbContext context, PrestamoService prestamoService, ReservaService reservaService)
        {
            _context = context;
            _prestamoService = prestamoService;
            _reservaService = reservaService;
        }

        public void FlujoPrestamo()
        {
            Console.WriteLine("\n--- REGISTRAR PRESTAMO ---");
            
            Console.Write("Ingresa NroSocio: ");
            if (!int.TryParse(Console.ReadLine(), out int nroSocio))
            {
                Console.WriteLine("Error: NroSocio debe ser un numero valido.");
                return;
            }

            var socio = _context.Socios.FirstOrDefault(s => s.NroSocio == nroSocio);
            if (socio == null)
            {
                Console.WriteLine("Error: El socio no existe.");
                return;
            }

            string isbn = BuscarLibro();
            if (string.IsNullOrEmpty(isbn))
            {
                return;
            }

            var resultado = _prestamoService.RegistrarPrestamo(nroSocio, isbn);
            
            if (!resultado.OfrecerReserva)
            {
                Console.WriteLine($"\n{resultado.Mensaje}");
            }
            else
            {
                Console.WriteLine($"\nAtención: {resultado.Mensaje}");
                Console.Write("¿Querés reservar el libro para cuando esté disponible? (s/n): ");
                var respuesta = Console.ReadLine()?.Trim().ToLower();
                if (respuesta == "s")
                {
                    var resultadoReserva = _reservaService.CrearReserva(nroSocio, isbn);
                    Console.WriteLine(resultadoReserva.Mensaje);
                }
            }
        }

        private string BuscarLibro()
        {
            Console.Write("Buscar libro (titulo o autor): ");
            var termino = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(termino))
            {
                Console.WriteLine("Debe ingresar un termino de busqueda.");
                return null;
            }

            var libros = _context.Libros
                .Where(l => l.Titulo.Contains(termino) || l.Autor.Contains(termino))
                .ToList();

            if (!libros.Any())
            {
                Console.WriteLine("No se encontraron libros que coincidan con la busqueda.");
                return null;
            }

            Console.WriteLine();
            var opcionesLibros = new Dictionary<int, Libro>();
            int index = 1;

            var estadoActivoId = _context.EstadoPrestamos
                .Where(e => e.Estado == "Activo")
                .Select(e => e.Id)
                .FirstOrDefault();

            foreach (var libro in libros)
            {
                opcionesLibros.Add(index, libro);

                var copiasActivas = _context.Prestamos
                    .Count(p => p.ISBN == libro.ISBN && p.EstadoPrestamoId == estadoActivoId);
                
                var disponibles = libro.CantidadCopias - copiasActivas;

                Console.WriteLine($" Opción [{index}]: {libro.Titulo} — {libro.Autor} | Copias disponibles: {disponibles}");
                index++;
            }

            Console.WriteLine();
            Console.Write(" Ingresa el número de la opción elegida: ");
            if (int.TryParse(Console.ReadLine(), out int seleccion) && opcionesLibros.ContainsKey(seleccion))
            {
                return opcionesLibros[seleccion].ISBN;
            }
            else
            {
                Console.WriteLine("Opcion invalida.");
                return null;
            }
        }

        public void FlujoDevolucion()
        {
            Console.WriteLine("\n--- REGISTRAR DEVOLUCION ---");

            Console.Write("Ingresa NroSocio: ");
            if (!int.TryParse(Console.ReadLine(), out int nroSocio))
            {
                Console.WriteLine("Error: NroSocio debe ser un numero valido.");
                return;
            }

            var socio = _context.Socios.FirstOrDefault(s => s.NroSocio == nroSocio);
            if (socio == null)
            {
                Console.WriteLine("Error: El socio no existe.");
                return;
            }

            var prestamosActivos = _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.NroSocio == nroSocio && p.FechaDevolucion == null)
                .ToList();

            if (!prestamosActivos.Any())
            {
                Console.WriteLine("El socio no tiene prestamos activos.");
                return;
            }

            Console.WriteLine("\nPrestamos activos:");
            foreach (var p in prestamosActivos)
            {
                var vencido = p.FechaVencimiento < DateTime.Today;
                var marca = vencido ? " - VENCIDO" : "";
                Console.WriteLine($" [{p.Id}] {p.Libro.Titulo} — vence {p.FechaVencimiento:dd/MM/yyyy}{marca}");
            }

            Console.WriteLine();
            Console.Write("Ingresa el Id del prestamo a devolver: ");
            if (!int.TryParse(Console.ReadLine(), out int idPrestamo))
            {
                Console.WriteLine("Error: Id invalido.");
                return;
            }

            var resultado = _prestamoService.RegistrarDevolucion(idPrestamo).GetAwaiter().GetResult();
            Console.WriteLine($"\n{resultado.Mensaje}");
        }

        public void FlujoReserva()
        {
            Console.WriteLine("\n--- REGISTRAR RESERVA ---");

            Console.Write("Ingresa NroSocio: ");
            if (!int.TryParse(Console.ReadLine(), out int nroSocio))
            {
                Console.WriteLine("Error: NroSocio debe ser un numero valido.");
                return;
            }

            string isbn = BuscarLibro();
            if (string.IsNullOrEmpty(isbn))
            {
                return;
            }

            var resultado = _reservaService.CrearReserva(nroSocio, isbn);
            Console.WriteLine($"\n{resultado.Mensaje}");
        }

        public void VerLibrosDisponibles()
        {
            Console.WriteLine("\n--- LIBROS DISPONIBLES ---");

            var disponibles = _context.Libros
                .Select(l => new
                {
                    l.Titulo,
                    l.Autor,
                    Disponibles = l.CantidadCopias - l.Prestamos.Count(p => p.FechaDevolucion == null)
                })
                .Where(l => l.Disponibles > 0)
                .ToList();

            if (!disponibles.Any())
            {
                Console.WriteLine("No hay libros con copias disponibles.");
                return;
            }

            foreach (var libro in disponibles)
            {
                Console.WriteLine($"  {libro.Titulo} ({libro.Autor}) — {libro.Disponibles} disponible(s)");
            }
        }

        public void FlujoSocio()
        {
            Console.WriteLine("\n--- DETALLE DE SOCIO ---");

            Console.Write("Ingresa NroSocio: ");
            if (!int.TryParse(Console.ReadLine(), out int nroSocio))
            {
                Console.WriteLine("Error: NroSocio debe ser un numero valido.");
                return;
            }

            var socio = _context.Socios
                .Include(s => s.TipoSocio)
                .FirstOrDefault(s => s.NroSocio == nroSocio);

            if (socio == null)
            {
                Console.WriteLine("Error: El socio no existe.");
                return;
            }

            Console.WriteLine($"\n  Nombre: {socio.Nombre} {socio.Apellido}");
            Console.WriteLine($"  Email: {socio.Email}");
            Console.WriteLine($"  Tipo: {socio.TipoSocio.Clase} (max {socio.TipoSocio.MaxLibrosSimultaneos} libros, {socio.TipoSocio.DiasPrestamo} dias)");
            Console.WriteLine($"  Estado: {(socio.Activo ? "Activo" : "Inactivo")}");

            var prestamos = _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.NroSocio == nroSocio && p.FechaDevolucion == null)
                .ToList();

            Console.WriteLine($"\n  Prestamos activos: {prestamos.Count}");
            foreach (var p in prestamos)
            {
                var vencido = p.FechaVencimiento < DateTime.Today;
                var marca = vencido ? " - VENCIDO" : "";
                Console.WriteLine($"    [{p.Id}] {p.Libro.Titulo} — vence {p.FechaVencimiento:dd/MM/yyyy}{marca}");
            }

            var totalMultas = _context.Prestamos
                .Where(p => p.NroSocio == nroSocio && p.MultaGenerada > 0 && !p.MultaPagada)
                .Select(p => p.MultaGenerada)
                .ToList()
                .Sum(m => m ?? 0);

            if (totalMultas > 0)
                Console.WriteLine($"\n  Multas pendientes: ${totalMultas:F2}");

            var reservasPendientes = _context.Reservas
                .Include(r => r.Libro)
                .Include(r => r.EstadoReserva)
                .Where(r => r.NroSocio == nroSocio && r.EstadoReserva.Descripcion == "Pendiente")
                .ToList();

            if (reservasPendientes.Any())
            {
                Console.WriteLine($"\n  Reservas pendientes: {reservasPendientes.Count}");
                foreach (var r in reservasPendientes)
                    Console.WriteLine($"    [{r.Id}] {r.Libro.Titulo} — {r.FechaReserva:dd/MM/yyyy}");
            }
        }
    }
}
