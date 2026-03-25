# Guía de base de datos (SQL Server)

Este documento explica cómo preparar la base de datos de **ClinicScheduling API**, incluyendo migraciones, stored procedures y restauración desde backup.

---

## 1) Resumen funcional de la capa de datos

La solución combina dos enfoques:

1. **Entity Framework Core** para estructura base (tablas, relaciones, constraints).
2. **Stored Procedures** para lógica transaccional y de disponibilidad de citas.

Esto permite mantener una API clara en C# y reglas críticas de agenda cerca de SQL Server.

---

## 2) Stored Procedures incluidos

Ubicación:

- `ClinicScheduling.Api/Docs/SqlServer/SpDb/`

### 2.1 `sp_CreateAppointment.sql`

Responsable de crear citas aplicando reglas de negocio, entre ellas:

- evitar citas duplicadas,
- impedir traslapes para el mismo médico,
- validar que la cita esté dentro del horario configurado,
- respetar duración de la especialidad,
- rechazar datos fuera de rango.

### 2.2 `sp_GetAppointmentsByDoctor.sql`

Consulta calendario de citas por médico:

- filtra por `DoctorId`,
- devuelve citas activas,
- facilita la vista de agenda del profesional.

### 2.3 `sp_GetDoctorAvailability.sql`

Calcula slots disponibles para una fecha:

- recibe `DoctorId` y fecha,
- considera duración por especialidad,
- cruza horario laboral y citas existentes,
- retorna bloques realmente disponibles.

---

## 3) Opción A: crear BD con migraciones EF Core

### 3.1 Instalar herramienta EF (si aplica)

```bash
dotnet tool install --global dotnet-ef
```

### 3.2 Crear migración inicial (si aún no existe)

```bash
dotnet ef migrations add InitialCreate --project ClinicScheduling.Api
```

### 3.3 Aplicar migraciones

```bash
dotnet ef database update --project ClinicScheduling.Api
```

Al finalizar tendrás:

- tablas,
- relaciones,
- índices,
- constraints.

### 3.4 Ejecutar Stored Procedures

Después de migrar, ejecuta manualmente los scripts de `SpDb` en SQL Server (SSMS, Azure Data Studio o `sqlcmd`).

Ejemplo con `sqlcmd`:

```bash
sqlcmd -S <server>,1433 -U sa -P "<password>" -d ClinicScheduling -i sp_CreateAppointment.sql -C
sqlcmd -S <server>,1433 -U sa -P "<password>" -d ClinicScheduling -i sp_GetAppointmentsByDoctor.sql -C
sqlcmd -S <server>,1433 -U sa -P "<password>" -d ClinicScheduling -i sp_GetDoctorAvailability.sql -C
```

---

## 4) Opción B: restaurar base desde backup (.bak)

Archivo de respaldo:

- `ClinicScheduling.Api/Docs/SqlServer/BackupDb/ClinicScheduling.bak`

> Verifica ruta exacta en tu entorno antes de ejecutar restauración.

### 4.1 Restauración en SQL Server (host/Windows)

```sql
RESTORE DATABASE ClinicScheduling
FROM DISK = 'C:\Ruta\ClinicScheduling.bak'
WITH REPLACE;
```

### 4.2 Restauración en SQL Server Docker

1. Copiar backup al contenedor:

```bash
docker cp ClinicScheduling.bak sqlserver:/var/opt/mssql/data/ClinicScheduling.bak
```

2. Ejecutar restore:

```bash
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "<password>" -C \
  -Q "RESTORE DATABASE ClinicScheduling FROM DISK = '/var/opt/mssql/data/ClinicScheduling.bak' WITH REPLACE"
```

---

## 5) Connection string recomendada

```text
Server=<host>,1433;Database=ClinicScheduling;User Id=<user>;Password=<password>;TrustServerCertificate=True;Encrypt=False
```

### Consideraciones

- Si API y SQL Server corren en contenedores distintos, usar hostname de red Docker (no `localhost`).
- Para producción, proteger secretos y evitar credenciales en código.

---

## 6) Verificación posterior a la creación

Checklist mínimo:

- [ ] La base `ClinicScheduling` existe.
- [ ] Tablas principales creadas.
- [ ] Los 3 stored procedures están presentes.
- [ ] La API conecta sin error de login/timeout.
- [ ] Endpoints de agenda responden correctamente.

---

## 7) Diagrama de datos

El modelo visual se encuentra en:

- `database diagram.drawio`

Puede abrirse con:

- https://app.diagrams.net/

Úsalo como referencia rápida para relaciones y navegación de entidades.

