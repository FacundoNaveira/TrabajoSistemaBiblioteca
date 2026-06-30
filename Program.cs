using System;
using System.IO;
using System.Linq;
using Biblioteca.Data;
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

            var prestamoService = new Biblioteca.Services.PrestamoService(context);
            var reservaService = new Biblioteca.Services.ReservaService(context);
            
            var consoleFlows = new Biblioteca.UI.ConsoleFlows(context, prestamoService, reservaService);
            
            // Iniciamos el flujo que acabamos de crear
            consoleFlows.FlujoPrestamo();
        }
    }
}
