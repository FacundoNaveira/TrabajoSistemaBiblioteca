using System;
using System.Linq;
using System.Threading.Tasks;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services
{
    public class ReservaService
    {
        private readonly BibliotecaDbContext _context;

        public ReservaService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public ResultadoOperacion CrearReserva(int nroSocio, string isbn)
        {
            var socio = _context.Socios.FirstOrDefault(s => s.NroSocio == nroSocio);
            if (socio == null)
                return new ResultadoOperacion { Mensaje = "El socio no existe." };

            if (!socio.Activo)
                return new ResultadoOperacion { Mensaje = "El socio esta inactivo y no puede reservar." };

            var estadoPendienteId = _context.EstadoReservas
                .Where(e => e.Descripcion == "Pendiente")
                .Select(e => e.Id)
                .First();

            var reservaExistente = _context.Reservas
                .Any(r => r.NroSocio == nroSocio && r.ISBN == isbn && r.EstadoReservaId == estadoPendienteId);

            if (reservaExistente)
                return new ResultadoOperacion { Mensaje = "El socio ya tiene una reserva pendiente para este libro." };

            var reserva = new Reserva
            {
                NroSocio = nroSocio,
                ISBN = isbn,
                FechaReserva = DateTime.Now,
                EstadoReservaId = estadoPendienteId
            };

            _context.Reservas.Add(reserva);
            _context.SaveChanges();

            return new ResultadoOperacion { Exitoso = true, Mensaje = "Reserva registrada. Seras notificado cuando este disponible." };
        }

        public async Task ProcesarReservasPendientes(string isbn)
        {
            var estadoPendienteId = _context.EstadoReservas
                .Where(e => e.Descripcion == "Pendiente")
                .Select(e => e.Id)
                .First();

            var estadoCumplidaId = _context.EstadoReservas
                .Where(e => e.Descripcion == "Cumplida")
                .Select(e => e.Id)
                .First();

            var reserva = await _context.Reservas
                .Include(r => r.Socio)
                .Include(r => r.Libro)
                .Where(r => r.ISBN == isbn && r.EstadoReservaId == estadoPendienteId)
                .OrderBy(r => r.FechaReserva)
                .FirstOrDefaultAsync();

            if (reserva != null)
            {
                reserva.EstadoReservaId = estadoCumplidaId;
                await _context.SaveChangesAsync();

                Console.WriteLine($"\n[Notificacion] La reserva del socio {reserva.Socio.Nombre} {reserva.Socio.Apellido} (NroSocio: {reserva.Socio.NroSocio})");
                Console.WriteLine($"para '{reserva.Libro.Titulo}' esta lista para retiro.\n");
            }
        }
    }
}
