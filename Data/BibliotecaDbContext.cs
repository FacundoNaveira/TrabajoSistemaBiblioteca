using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data
{
    public class BibliotecaDbContext : DbContext
    {
        public DbSet<TipoSocio> TipoSocios { get; set; } = null!;
        public DbSet<EstadoPrestamo> EstadoPrestamos { get; set; } = null!;
        public DbSet<EstadoReserva> EstadoReservas { get; set; } = null!;
        public DbSet<Libro> Libros { get; set; } = null!;
        public DbSet<Socio> Socios { get; set; } = null!;
        public DbSet<Prestamo> Prestamos { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;

        public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Libro>(entity =>
            {
                entity.HasKey(l => l.ISBN);
                entity.Property(l => l.ISBN).ValueGeneratedNever();
                entity.Property(l => l.Titulo).IsRequired();
                entity.Property(l => l.Autor).IsRequired();
                entity.Property(l => l.Genero).IsRequired();
                entity.Property(l => l.CantidadCopias).IsRequired();
            });

            modelBuilder.Entity<Socio>(entity =>
            {
                entity.HasKey(s => s.NroSocio);
                entity.Property(s => s.NroSocio).ValueGeneratedNever();
                entity.Property(s => s.Nombre).IsRequired();
                entity.Property(s => s.Apellido).IsRequired();
                entity.Property(s => s.Email).IsRequired();
                entity.Property(s => s.TipoSocioId).IsRequired();
                entity.Property(s => s.Activo).IsRequired().HasColumnType("INTEGER");

                entity.HasOne(s => s.TipoSocio)
                      .WithMany(t => t.Socios)
                      .HasForeignKey(s => s.TipoSocioId);
            });

            modelBuilder.Entity<TipoSocio>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Clase).IsRequired();
                entity.Property(t => t.MaxLibrosSimultaneos).IsRequired();
                entity.Property(t => t.DiasPrestamo).IsRequired();
                entity.Property(t => t.MultaPorDia).IsRequired().HasPrecision(10, 2);
            });

            modelBuilder.Entity<EstadoPrestamo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Estado).IsRequired();
            });

            modelBuilder.Entity<EstadoReserva>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descripcion).IsRequired();
            });

            modelBuilder.Entity<Prestamo>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.NroSocio).IsRequired();
                entity.Property(p => p.ISBN).IsRequired();
                entity.Property(p => p.FechaPrestamo).IsRequired().HasColumnType("TEXT");
                entity.Property(p => p.FechaVencimiento).IsRequired().HasColumnType("TEXT");
                entity.Property(p => p.FechaDevolucion).HasColumnType("TEXT");
                entity.Property(p => p.EstadoPrestamoId).IsRequired();
                entity.Property(p => p.MultaGenerada).HasPrecision(10, 2);
                entity.Property(p => p.MultaPagada).IsRequired().HasColumnType("INTEGER");

                entity.HasOne(p => p.Socio)
                      .WithMany(s => s.Prestamos)
                      .HasForeignKey(p => p.NroSocio);

                entity.HasOne(p => p.Libro)
                      .WithMany(l => l.Prestamos)
                      .HasForeignKey(p => p.ISBN);

                entity.HasOne(p => p.EstadoPrestamo)
                      .WithMany(e => e.Prestamos)
                      .HasForeignKey(p => p.EstadoPrestamoId);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.NroSocio).IsRequired();
                entity.Property(r => r.ISBN).IsRequired();
                entity.Property(r => r.FechaReserva).IsRequired().HasColumnType("TEXT");
                entity.Property(r => r.EstadoReservaId).IsRequired();

                entity.HasOne(r => r.Socio)
                      .WithMany(s => s.Reservas)
                      .HasForeignKey(r => r.NroSocio);

                entity.HasOne(r => r.Libro)
                      .WithMany(l => l.Reservas)
                      .HasForeignKey(r => r.ISBN);

                entity.HasOne(r => r.EstadoReserva)
                      .WithMany(e => e.Reservas)
                      .HasForeignKey(r => r.EstadoReservaId);
            });
        }
    }
}
