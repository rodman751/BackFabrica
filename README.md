# BackFabrica - Dynamic Database API

> .NET 8 Web API with layered architecture for dynamic CRUD operations, multi-database support, and runtime schema introspection over SQL Server.

---

## Repositories

| Project | Repository |
|---------|-----------|
| **Backend** (.NET 8 Web API) | This repository |
| **Frontend** (Flutter Mobile App) | [github.com/DiegoCuaycal/DataGest](https://github.com/DiegoCuaycal/DataGest) |

---

## Overview

**BackFabrica** is the backend for the **DataGest** platform. It exposes a RESTful API that dynamically generates SQL queries at runtime based on database schema metadata, eliminating the need for static CRUD code per table.

The core capability: given any SQL Server database, the API introspects its schema (tables, columns, primary keys, foreign keys, identity columns) via a stored procedure and builds parameterized INSERT, UPDATE, SELECT, and GET BY ID queries on the fly — validated against the schema to prevent injection.

It also includes domain-specific controllers for three business modules (Education, Healthcare, Products) and an MVC web app (GenAPK) for compiling customized Flutter APKs.

---

## Tech Stack

**Runtime:** .NET 8, ASP.NET Core Web API, ASP.NET Core MVC

**Data Access:** Dapper 2.1.66, Microsoft.Data.SqlClient 6.1.3, SQL Server

**Security:** JWT Bearer Authentication (HMAC-SHA256), System.IdentityModel.Tokens.Jwt

**Docs:** Swashbuckle / Swagger

---

## Solution Structure

Layered architecture across 4 projects:

```
BackFabrica.sln
├── BackFabrica/        # API Layer - REST Controllers, DI, Swagger
├── CapaDapper/         # Data Access - Dapper repos, DTOs, entities, dynamic connections
├── Services/           # Business Logic - JWT auth, APK builder
└── GenAPK/             # MVC Web App - APK generator UI
```

### BackFabrica (API)

Controllers: `AuthController`, `DynamicCrudController`, `SchemaController`, `ProductosController`, `EducacionController`, `SaludController`

### CapaDapper (Data Access)

- **Cadena/** - `DbConnectionFactory` resolves connection profiles dynamically per request via `X-Connection-Profile` header or session
- **DataService/** - `DynamicCrudService` (generic CRUD), `DbMetadataRepository` (schema introspection), domain repositories
- **Dtos/** - `DbSchema`, `DynamicRequestDto`, `LoginResponseDto`, `RequestCrearModuloDto`
- **Entidades/** - Domain entities for Education, Products, Healthcare

### Services (Business Logic)

- `AuthService` - JWT token generation (HMAC-SHA256, 8h expiration) with claims
- `AuthRepository` - Credential validation via `sp_ValidarLoginFinal` stored procedure
- `ApkBuilderService` - Server-side Flutter APK compilation

---

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/login` | Login with `X-DbName`, `X-Usuario`, `X-Password` headers. Returns JWT + user data |
| POST | `/api/Auth/crear-modulo` | Create new database module from JSON schema |

### Schema Introspection

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Schema/databases` | List all available databases (excludes system DBs) |
| GET | `/api/Schema/generate?db={name}` | Get full schema JSON via `sp_GetDatabaseSchema` |

### Dynamic CRUD (V2)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/DynamicCrud/V2/GETALL` | Fetch all records from any table |
| POST | `/api/DynamicCrud/V2/GETBYID/{id}` | Fetch single record by PK |
| POST | `/api/DynamicCrud/V2/CREADTE` | Insert new record |
| POST | `/api/DynamicCrud/V2/UPDATE` | Update existing record |

All V2 endpoints receive a `DynamicRequestDto` with `Schema` (database metadata) and `Data` (record fields). The schema drives query generation and validation.

### Domain Controllers

- **ProductosController** - Products, Categories, Suppliers, Inventory (full CRUD + stock adjustment)
- **EducacionController** - Students, Professors, Courses, Enrollments, Grades (with `[Authorize]`)
- **SaludController** - Patients, Doctors, Appointments, Diagnoses

All domain endpoints require the `X-DbName` header.

---

## Key Architecture Decisions

**Dynamic Connection Strings** - The `DbConnectionFactory` reads `X-Connection-Profile` from the request header, resolves the template from `appsettings.json`, and injects the database name via `string.Format(template, CurrentDb)`. Supports multiple SQL Server instances (Local, Azure, Remote).

**Schema-Driven CRUD** - `DynamicCrudService` validates table/column names against the `DbSchema` DTO before building SQL. Identity columns are auto-excluded from INSERT. Column names are bracket-escaped `[ColumnName]`. PascalCase/camelCase input is mapped to actual column names via case-insensitive matching and snake_case conversion.

**Scoped Database Context** - `IDatabaseContext.CurrentDb` is set per-request, ensuring each API call targets the correct database without shared state.

**Stored Procedures** - `sp_GetDatabaseSchema` (schema introspection), `sp_ValidarLoginFinal` (authentication), `sp_Master_CrearModuloCompleto` (new module creation).

---

## Design Patterns

**Repository Pattern** - Interfaces per domain (`IProductosRepository`, `IEducacionRepository`, `ISaludRepository`, `IDbMetadataRepository`)

**Dependency Injection** - All services registered as `Scoped` in `Program.cs`

**Factory Pattern** - `DbConnectionFactory.CreateConnection()` builds connections dynamically

**DTO Pattern** - `DbSchema`, `DynamicRequestDto`, `LoginResponseDto` for API contracts

**Service Layer** - `AuthService` handles JWT generation, separated from data access

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ (or Azure SQL Database)

### Setup

```bash
git clone https://github.com/rodman751/BackFabrica.git
cd BackFabrica
dotnet restore
```

Configure `BackFabrica/appsettings.json` with your connection profiles:

```json
{
  "ConnectionStrings": {
    "TemplateConnection": "Server=<host>;Database={0};User Id=<user>;Password=<pass>;..."
  },
  "ConnectionProfiles": {
    "Local": { "ConnectionString": "Server=localhost;Database={0};..." },
    "Azure": { "ConnectionString": "Server=<azure-host>;Database={0};..." }
  }
}
```

### Run

```bash
dotnet run --project BackFabrica
```

Swagger UI available at `https://localhost:{port}/swagger`.

---

*Built with .NET 8, Dapper, SQL Server, JWT, and Layered Architecture*
