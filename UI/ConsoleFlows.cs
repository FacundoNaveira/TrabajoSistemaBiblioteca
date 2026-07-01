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
            Console.WriteLine($"\n{resultado.Mensaje}");

            if (resultado.OfrecerReserva)
            {
                Console.Write("No hay copias disponibles. Queres reservarlo? (s/n): ");
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

                Console.WriteLine($"[{index}] {libro.Titulo} — {libro.Autor} | Disponibles: {disponibles}");
                index++;
            }

            Console.WriteLine();
            Console.Write("Elegi un numero: ");
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
    }
}
