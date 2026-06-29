-- ============================================================
-- SISTEMA DE GESTIÓN DE BIBLIOTECA MUNICIPAL
-- Creación de tablas - Sin datos
-- Motor: SQLite | Proyecto: .NET 8 + EF Core 8
-- ============================================================

PRAGMA foreign_keys = ON;

-- ============================================================
-- Tablas de catálogo
-- ============================================================

-- Tipos de socio: define límites y condiciones de préstamo por categoría
CREATE TABLE TipoSocio (
    Id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    Clase                TEXT    NOT NULL,
    MaxLibrosSimultaneos INTEGER NOT NULL,
    DiasPrestamo         INTEGER NOT NULL,
    MultaPorDia          NUMERIC(10,2) NOT NULL
);

-- Estados posibles de un préstamo
CREATE TABLE EstadoPrestamo (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Estado TEXT NOT NULL
);

-- Estados posibles de una reserva
CREATE TABLE EstadoReserva (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Descripcion TEXT NOT NULL
);

-- ============================================================
-- Tablas principales
-- ============================================================

-- Libros del catálogo de la biblioteca
CREATE TABLE Libro (
    ISBN           TEXT PRIMARY KEY,
    Titulo         TEXT    NOT NULL,
    Autor          TEXT    NOT NULL,
    Genero         TEXT    NOT NULL,
    CantidadCopias INTEGER NOT NULL
);

-- Socios de la biblioteca
CREATE TABLE Socio (
    NroSocio    INTEGER PRIMARY KEY,
    Nombre      TEXT    NOT NULL,
    Apellido    TEXT    NOT NULL,
    Email       TEXT    NOT NULL,
    TipoSocioId INTEGER NOT NULL REFERENCES TipoSocio(Id),
    Activo      INTEGER NOT NULL
);

-- Préstamos de libros a socios
CREATE TABLE Prestamo (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    NroSocio         INTEGER NOT NULL REFERENCES Socio(NroSocio),
    ISBN             TEXT    NOT NULL REFERENCES Libro(ISBN),
    FechaPrestamo    TEXT    NOT NULL,
    FechaVencimiento TEXT    NOT NULL,
    FechaDevolucion  TEXT,
    EstadoPrestamoId INTEGER NOT NULL REFERENCES EstadoPrestamo(Id),
    MultaGenerada    NUMERIC(10,2)
);

-- Reservas de libros por socios
CREATE TABLE Reserva (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    NroSocio        INTEGER NOT NULL REFERENCES Socio(NroSocio),
    ISBN            TEXT    NOT NULL REFERENCES Libro(ISBN),
    FechaReserva    TEXT    NOT NULL,
    EstadoReservaId INTEGER NOT NULL REFERENCES EstadoReserva(Id)
);
