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

            var disponibles = context.Libros
                .Select(l => new
                {
                    l.Titulo,
                    l.Autor,
                    Disponibles = l.CantidadCopias - l.Prestamos.Count(p => p.FechaDevolucion == null)
                })
                .Where(l => l.Disponibles > 0)
                .ToList();

            Console.WriteLine("Libros con copias disponibles:");
            foreach (var libro in disponibles)
            {
                Console.WriteLine($"  {libro.Titulo} ({libro.Autor}) — {libro.Disponibles} disponible(s)");
            }
        }
    }
}
