# ClinicScheduling API

API REST para gestión de agenda clínica (usuarios, médicos, pacientes, horarios y citas), construida en **ASP.NET Core 9** con **Entity Framework Core + SQL Server** y autenticación por **JWT**.

> Este repositorio corresponde al backend. El frontend (React) vive en un repositorio separado.

---

## 1) Objetivo del proyecto

El sistema permite administrar la operación principal de una clínica:

- Alta/baja/cambios y consulta de:
  - Roles
  - Usuarios
  - Especialidades
  - Médicos
  - Pacientes
  - Horarios de médico
  - Citas
- Consulta de disponibilidad por médico y fecha.
- Protección de endpoints con autenticación/autorización JWT.
- Exposición de documentación OpenAPI (Swagger).

---

## 2) Stack tecnológico

- **.NET 9 (net9.0)**
- **ASP.NET Core Web API**
- **Entity Framework Core 9 (SQL Server)**
- **Stored Procedures en SQL Server** para reglas clave de agenda
- **JWT Bearer Authentication**
- **NUnit + Microsoft.AspNetCore.Mvc.Testing** para pruebas de integración
- **Docker** para empaquetado y despliegue
- **Azure DevOps Pipeline** para CI/CD

---

## 3) Arquitectura (visión rápida)

El proyecto sigue una separación por capas orientada a mantenibilidad:

- `Controllers/`: endpoints HTTP.
- `Models/Repositories` y `Models/IRepositories`: acceso a datos y reglas de aplicación.
- `Common/Database`: contexto de EF y entidades.
- `Common/Dtos`, `Common/MapperProfiles`: contratos de entrada/salida y mapeos.
- `Common/Security`: servicios de autenticación/JWT.

Si quieres detalle operacional de base de datos, revisa:

- `ClinicScheduling.Api/Docs/Crear_base_de_datos.md`

---

## 4) Requisitos para desarrollo local

- .NET SDK 9.x
- SQL Server (local, remoto o en Docker)
- (Opcional) Docker para ejecutar la API en contenedor

---

## 5) Ejecución local (sin Docker)

1. Restaurar dependencias:

```bash
dotnet restore
```

2. Configurar `ConnectionStrings:ClinicSchedulingDb` y `Jwt` en `ClinicScheduling.Api/appsettings.json` (o con variables de entorno).

3. Ejecutar API:

```bash
dotnet run --project ClinicScheduling.Api
```

4. Abrir Swagger:

- `http://localhost:<puerto>/swagger`

> Nota: en este proyecto Swagger está habilitado también fuera de Development.

---

## 6) Ejecución con Docker

Desde la carpeta `ClinicScheduling.Api/`:

1. Construir imagen:

```bash
docker build -t clinicscheduling-api .
```

2. Ejecutar contenedor:

```bash
docker run -d \
  --name clinicscheduling-api \
  -p 4444:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  clinicscheduling-api
```

Swagger quedará disponible en:

- `http://localhost:4444/swagger/index.html`

---

## 7) Configuración de conexión a base de datos

Puedes configurar la conexión a SQL Server de dos formas:

1. **appsettings.json**
2. **Variables de entorno** (recomendado para CI/CD y producción)

Ejemplo:

```json
"ConnectionStrings": {
  "ClinicSchedulingDb": "Server=localhost,1433;Database=ClinicScheduling;User Id=sa;Password=<password>;TrustServerCertificate=True;Encrypt=False"
}
```

### Recomendaciones importantes

- En Docker, evita `localhost` cuando SQL Server está en otro contenedor. Usa el nombre del servicio/host de red (ej. `sqlserver,1433`).
- No hardcodear credenciales de producción en repositorio.
- Usar secretos del pipeline/entorno (Azure DevOps variable groups, secretos de host, etc.).

---

## 8) CI/CD (Azure DevOps)

El pipeline (`azure-pipelines.yml`) está configurado para rama `main` y realiza:

1. Selección de SDK .NET 9.
2. Restore y build del proyecto.
3. Copia de código por SSH al servidor objetivo.
4. Build de imagen Docker en el servidor.
5. Reemplazo controlado del contenedor (`stop/rm/run`).

Esto permite despliegue continuo sin intervención manual por cada cambio.

---

## 9) Base de datos y migraciones

Consulta guía detallada en:

- `ClinicScheduling.Api/Docs/Crear_base_de_datos.md`

Ahí se documenta:

- creación por migraciones EF,
- ejecución de stored procedures,
- restauración desde backup `.bak`,
- y diagrama de modelo.

---

## 10) Pruebas

El repositorio incluye pruebas de integración en `ClinicScheduling.Api.Tests`.

Guía de ejecución:

- `ClinicScheduling.Api.Tests/Como_ejecutar_pruebas.md`

---

## 11) Repositorios relacionados

- Frontend (React):
  - https://github.com/saulkali/ClinicSchedulingFrontEnd.git
- Backend (este repositorio):
  - https://github.com/saulkali/ClinicSchedulingAPI.git

---


## 12) Docker Compose

Se agregó una guía para levantar el stack base con Compose:

- `docker-compose.yml`
- `DOCKER_COMPOSE.md`

> Incluye backend + SQL Server en este repo. El frontend se documenta como integración externa.

---

