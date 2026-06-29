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

-- ============================================================
-- DATOS INICIALES
-- ============================================================

-- Tipos de socio
INSERT INTO TipoSocio (Id, Clase, MaxLibrosSimultaneos, DiasPrestamo, MultaPorDia) VALUES
(1, 'Común',      3,  7, 150.00),
(2, 'Estudiante', 5, 14,  75.00),
(3, 'Docente',    8, 30,  50.00);

-- Estados de préstamo
INSERT INTO EstadoPrestamo (Id, Estado) VALUES
(1, 'Activo'),
(2, 'Devuelto'),
(3, 'Vencido');

-- Estados de reserva
INSERT INTO EstadoReserva (Id, Descripcion) VALUES
(1, 'Pendiente'),
(2, 'Cumplida'),
(3, 'Cancelada');

-- Libros del catálogo
INSERT INTO Libro (ISBN, Titulo, Autor, Genero, CantidadCopias) VALUES
('978-0441172719', 'Dune',                            'Frank Herbert',              'Ciencia Ficción',     2),
('978-0451524935', '1984',                            'George Orwell',              'Distopía',            1),
('978-0307474728', 'Cien Años de Soledad',            'Gabriel García Márquez',     'Realismo Mágico',     3),
('978-0061120084', 'Matar un Ruiseñor',               'Harper Lee',                 'Drama',               2),
('978-0316769488', 'El Guardián en el Centeno',       'J.D. Salinger',              'Novela',              2);

-- Socios (al menos uno de cada tipo, uno inactivo)
INSERT INTO Socio (NroSocio, Nombre, Apellido, Email, TipoSocioId, Activo) VALUES
(1, 'Juan',   'Pérez',      'jperez@email.com',      1, 1),
(2, 'María',  'García',     'mgarcia@email.com',     2, 1),
(3, 'Carlos', 'López',      'clopez@email.com',      3, 1),
(4, 'Ana',    'Martínez',   'amartinez@email.com',   1, 0),
(5, 'Pedro',  'Rodríguez',  'prodriguez@email.com',   2, 1);

-- Préstamos: 2 activos (Dune → 0 copias disponibles), 1 devuelto en término, 1 vencido
INSERT INTO Prestamo (Id, NroSocio, ISBN, FechaPrestamo, FechaVencimiento, FechaDevolucion, EstadoPrestamoId, MultaGenerada) VALUES
(1, 1, '978-0441172719', '2026-06-25', '2026-07-02', NULL,        1, NULL),
(2, 2, '978-0441172719', '2026-06-26', '2026-07-10', NULL,        1, NULL),
(3, 3, '978-0307474728', '2026-06-10', '2026-07-10', '2026-06-25', 2, 0.00),
(4, 5, '978-0451524935', '2026-06-01', '2026-06-15', NULL,        3, NULL);

-- Reservas pendientes sobre Dune (0 copias disponibles)
INSERT INTO Reserva (Id, NroSocio, ISBN, FechaReserva, EstadoReservaId) VALUES
(1, 3, '978-0441172719', '2026-06-27', 1),
(2, 5, '978-0441172719', '2026-06-28', 1);
