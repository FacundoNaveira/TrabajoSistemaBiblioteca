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
            var reporteService = new ReporteService(context);
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
║  6. Reportes y consultas     ║
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
                        case "6":
                            bool volver = false;
                            while (!volver)
                            {
                                Console.WriteLine(@"
╔══════════════════════════════════════╗
║         REPORTES Y CONSULTAS         ║
╠══════════════════════════════════════╣
║  1. Libros más prestados             ║
║  2. Socios con multas pendientes     ║
║  3. Préstamos vencidos               ║
║  4. Disponibilidad de un libro       ║
║  5. Historial de un socio            ║
║  0. Volver al menú principal         ║
╚══════════════════════════════════════╝");
                                Console.Write("Opcion: ");
                                var repInput = Console.ReadLine();
                                switch (repInput)
                                {
                                    case "1":
                                        reporteService.LibrosMasPrestados().Wait();
                                        break;
                                    case "2":
                                        reporteService.SociosConMultasPendientes().Wait();
                                        break;
                                    case "3":
                                        reporteService.PrestamosVencidos().Wait();
                                        break;
                                    case "4":
                                        Console.Write("Buscar libro (título o ISBN): ");
                                        var busqueda = Console.ReadLine()?.Trim() ?? "";
                                        reporteService.DisponibilidadLibro(busqueda).Wait();
                                        break;
                                    case "5":
                                        Console.Write("Ingresar NroSocio: ");
                                        if (int.TryParse(Console.ReadLine(), out int nro))
                                        {
                                            reporteService.HistorialSocio(nro).Wait();
                                        }
                                        else
                                        {
                                            Console.WriteLine("Número inválido.");
                                        }
                                        break;
                                    case "0":
                                        volver = true;
                                        break;
                                    default:
                                        Console.WriteLine("Opcion invalida.");
                                        break;
                                }
                            }
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
