# BackFabrica API

API REST desarrollada en **.NET 8** con arquitectura por capas para la gestión de múltiples dominios de negocio.

## 📋 Descripción

BackFabrica es un backend modular que expone endpoints RESTful para tres dominios principales:

| Dominio | Entidades |
|---------|-----------|
| **📚 Educación** | Estudiantes, Profesores, Cursos, Inscripciones |
| **📦 Productos** | Productos, Categorías, Proveedores, Inventario |
| **🏥 Salud** | Pacientes, Médicos, Citas, Diagnósticos |

## 🏗️ Arquitectura

El proyecto sigue una arquitectura de N capas:

```
BackFabrica/
├── BackFabrica/        # Capa de presentación (API Controllers)
├── Dapper/             # Capa de acceso a datos
│   ├── Entidades/      # Modelos de dominio
│   ├── DataService/    # Repositorios con Dapper
│   ├── Cadena/         # Configuración de conexión
│   └── Dtos/           # Objetos de transferencia
└── Services/           # Capa de servicios (Autenticación)
```

## 🛠️ Tecnologías

- **.NET 8** - Framework principal
- **Dapper** - Micro ORM para acceso a datos
- **SQL Server** - Base de datos
- **Swagger** - Documentación de API
- **JWT** - Autenticación

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local o Azure)

### Configuración

1. Clona el repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd BackFabrica
   ```

2. Configura la cadena de conexión en `BackFabrica/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "TemplateConnection": "Server=<servidor>;Database={0};User Id=<usuario>;Password=<contraseña>;..."
     }
   }
   ```

3. Restaura dependencias y ejecuta:
   ```bash
   dotnet restore
   dotnet run --project BackFabrica
   ```

4. Accede a Swagger UI:
   ```
   https://localhost:<puerto>/swagger
   ```

## 📁 Estructura de Proyectos

| Proyecto | Descripción |
|----------|-------------|
| `BackFabrica` | API Controllers y configuración de host |
| `CapaDapper` | Entidades, repositorios y acceso a datos |
| `Services` | Servicios de autenticación |

## 🔌 Endpoints Principales

### Educación
- `GET/POST/PUT/DELETE /api/Educacion/*`

### Productos  
- `GET/POST/PUT/DELETE /api/Productos/*`

### Salud
- `GET/POST/PUT/DELETE /api/Salud/*`

### Autenticación
- `POST /api/Auth/login`

## Licencia

Este proyecto es de uso educativo/universitario.
