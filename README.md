# Sistema de Gestion de Biblioteca

Gestion de prestamos y reservas para una biblioteca municipal. Implementa reglas de negocio RN-01 a RN-07 usando .NET 8 + EF Core 8 + SQLite.

## Requisitos

- .NET 8 SDK

## Como levantar

```bash
git clone <repo>
cd TrabajoSistemaBiblioteca
dotnet restore
dotnet run
```

La base de datos y los datos de prueba se crean automaticamente al iniciar.

## Estructura del proyecto

```
TrabajoSistemaBiblioteca/
├── database/
│   └── biblioteca.sql          # Esquema DDL + datos de prueba
├── Models/                     # Entidades EF Core
│   ├── TipoSocio.cs
│   ├── EstadoPrestamo.cs
│   ├── EstadoReserva.cs
│   ├── Libro.cs
│   ├── Socio.cs
│   ├── Prestamo.cs
│   ├── Reserva.cs
│   └── ResultadoOperacion.cs
├── Data/
│   └── BibliotecaDbContext.cs  # DbContext + Fluent API
├── Services/
│   ├── PrestamoService.cs       # RN-01 a RN-06
│   └── ReservaService.cs        # RN-07
├── UI/
│   └── ConsoleFlows.cs          # Flujos de consola y menu
├── Program.cs                   # Punto de entrada
├── TrabajoSistemaBiblioteca.csproj
└── README.md
```

## Reglas de negocio

| RN | Regla |
|----|-------|
| RN-01 | Socio inactivo no puede pedir prestamos |
| RN-02 | Socio con multas pendientes no puede pedir prestamos |
| RN-03 | Sin copias disponibles se ofrece reserva |
| RN-04 | El socio no puede exceder su limite de libros simultaneos |
| RN-05 | Fecha de vencimiento segun dias de prestamo del tipo de socio |
| RN-06 | Multa por atraso = diasAtraso x MultaPorDia del tipo de socio |
| RN-07 | Al devolver un libro se procesa la reserva mas antigua |

