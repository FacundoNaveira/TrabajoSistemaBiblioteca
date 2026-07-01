using System;
using System.IO;
using Biblioteca.Data;
using Biblioteca.Services;
using Biblioteca.UI;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
                .UseSqlite("Data Source=biblioteca.db")
                .Options;

            using var context = new BibliotecaDbContext(options);
            context.Database.EnsureCreated();

            var seedSql = File.ReadAllText("database/biblioteca.sql");
            context.Database.ExecuteSqlRaw(seedSql);

            var prestamoService = new PrestamoService(context);
            var reservaService = new ReservaService(context);
            var consoleFlows = new ConsoleFlows(context, prestamoService, reservaService);

            consoleFlows.VerLibrosDisponibles();

            var salir = false;
            while (!salir)
            {
                Console.WriteLine(@"
╔══════════════════════════════╗
║    BIBLIOTECA MUNICIPAL      ║
╠══════════════════════════════╣
║  1. Ver libros disponibles   ║
║  2. Registrar prestamo       ║
║  3. Registrar devolucion     ║
║  4. Hacer una reserva        ║
║  5. Ver detalle de socio     ║
║  0. Salir                    ║
╚══════════════════════════════╝");

                Console.Write("Opcion: ");
                var input = Console.ReadLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            consoleFlows.VerLibrosDisponibles();
                            break;
                        case "2":
                            consoleFlows.FlujoPrestamo();
                            break;
                        case "3":
                            consoleFlows.FlujoDevolucion();
                            break;
                        case "4":
                            consoleFlows.FlujoReserva();
                            break;
                        case "5":
                            consoleFlows.FlujoSocio();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opcion invalida. Intente de nuevo.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inesperado: {ex.Message}");
                }
            }

            Console.WriteLine("Hasta luego.");
        }
    }
}
