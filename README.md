# Sicoin - Módulo de Contabilidad (Backend)

Este repositorio contiene la lógica de negocio y los servicios de la API para el sistema contable **Sicoin**. Está construido bajo una arquitectura limpia utilizando .NET 9 y Entity Framework Core.

## 🚀 Tecnologías
- **Framework:** .NET 9 (ASP.NET Core Web API)
- **ORM:** Entity Framework Core
- **Base de Datos:** PostgreSQL
- **Arquitectura:** Domain-Driven Design (DDD) simplificado

## 🛠️ Requisitos
- .NET SDK 9.0 o superior
- PostgreSQL (Instancia local o remota)
- Visual Studio 2022 o VS Code

## 🏁 Instrucciones para Correr el Proyecto

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/GaboMV/backendDashboard.git
   ```

2. **Configurar la Base de Datos:**
   - Asegúrate de tener una instancia de PostgreSQL corriendo.
   - Crea un archivo `appsettings.Development.json` en la carpeta `Sicoin.Contabilidad.Api/` (si no existe) y configura tu cadena de conexión:
     ```json
     {
       "ConnectionStrings": {
         "ContaDb": "Server=TU_SERVIDOR;Database=conta_db;User Id=TU_USUARIO;Password=TU_PASSWORD;Port=5432"
       }
     }
     ```

3. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```

4. **Ejecutar las migraciones (Opcional):**
   ```bash
   dotnet ef database update --project Sicoin.Contabilidad.Infrastructure --startup-project Sicoin.Contabilidad.Api
   ```

5. **Iniciar la aplicación:**
   ```bash
   dotnet run --project Sicoin.Contabilidad.Api
   ```

La API estará disponible por defecto en `http://localhost:5089/swagger` para explorar la documentación.

## 📁 Estructura del Proyecto
- **Sicoin.Contabilidad.Api:** Endpoints y controladores.
- **Sicoin.Contabilidad.Application:** Lógica de negocio, DTOs y servicios.
- **Sicoin.Contabilidad.Domain:** Entidades y reglas de negocio.
- **Sicoin.Contabilidad.Infrastructure:** Persistencia de datos y configuración de DbContext.
