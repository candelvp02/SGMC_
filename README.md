# SGMC - Sistema de Gestión de Citas Médicas

## Descripción
Sistema web para la gestión de citas médicas, desarrollado como proyecto académico.

## Equipo
- Candelaria Pereyra Villar (Líder)
- Hiroki Yano Romero
- Luis Álvaro Moneró

## Tecnologías
- **Lenguaje:** C# · .NET 9.0
- **Backend:** ASP.NET Core 9 (API RESTful)
- **Acceso a Datos:** EF Core 9
- **Base de Datos:** SQL Server 2022
- **Arquitectura:** MVC + Repository Pattern
- **Metodología:** SCRUM
- **CI/CD:** Azure DevOps Pipelines

## Estructura del Proyecto
- `SGMC.Api` — Capa de presentación (API)
- `SGMC.Application` — Lógica de negocio
- `SGMC.Domain` — Entidades del dominio
- `SGMC.Infrastructure` — Configuración e infraestructura
- `SGMC.Persistence` — Acceso a datos
- `SGMC.Web` — Interfaz web
