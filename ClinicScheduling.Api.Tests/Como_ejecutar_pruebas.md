# Guía de pruebas de integración

Este proyecto contiene pruebas de integración para validar el comportamiento real de la API (HTTP + capa de datos + reglas de negocio).

---

## 1) Alcance de las pruebas

Las pruebas cubren principalmente:

- CRUD de controladores principales.
- Validaciones de reglas de negocio.
- Flujos completos de endpoints.
- Integración con SQL Server.
- Ejecución de stored procedures de agenda.

No son pruebas unitarias puras: ejercitan la aplicación de extremo a extremo dentro del contexto de testing.

---

## 2) Requisitos previos

- .NET SDK 9.x
- SQL Server disponible en `localhost,1433` con credenciales válidas
- Usuario con permisos para crear base de datos temporal

La factoría de pruebas crea automáticamente una base efímera para cada ejecución.

---

## 3) Configuración técnica relevante

Archivo clave:

- `ClinicScheduling.Api.Tests/Integration/ClinicWebApplicationFactory.cs`

Esta clase se encarga de:

- reemplazar `DbContext` productivo por uno de pruebas,
- crear una base temporal con nombre único,
- ejecutar `EnsureCreated`,
- cargar stored procedures desde `ClinicScheduling.Api/Docs/SqlServer/SpDb`,
- insertar datos semilla para escenarios de test.

---

## 4) Ejecutar pruebas

Desde la raíz del repositorio:

```bash
dotnet test
```

O solo el proyecto de tests:

```bash
dotnet test ClinicScheduling.Api.Tests/ClinicScheduling.Api.Tests.csproj
```

---

## 5) Solución de problemas comunes

### Error de conexión a SQL Server

Verifica:

- que SQL Server esté corriendo,
- puerto `1433` accesible,
- credenciales correctas,
- certificado/trust si aplica.

### Fallo al crear stored procedures

Verifica rutas y existencia de:

- `sp_CreateAppointment.sql`
- `sp_GetAppointmentsByDoctor.sql`
- `sp_GetDoctorAvailability.sql`

### Errores por colisión de estado

Las pruebas intentan aislar estado con una base temporal por ejecución. Si hay interrupciones abruptas, limpiar bases temporales antiguas puede ayudar.

---

## 6) Buenas prácticas al agregar nuevas pruebas

- Nombrar pruebas por comportamiento esperado.
- Preparar datos mínimos por escenario.
- Evitar dependencias ocultas entre pruebas.
- Cubrir casos felices + errores esperados.
- Mantener las pruebas determinísticas.

