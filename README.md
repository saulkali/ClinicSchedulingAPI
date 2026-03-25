BASE DE DATOS Y STORED PROCEDURES

Dentro de la carpeta SqlServer se encuentran los Stored Procedures utilizados por el sistema.
Actualmente son tres:

1. sp_CreateAppointment.sql
   Este procedimiento se encarga de crear citas médicas e incluye las siguientes reglas de negocio:

- Evita citas duplicadas
- Impide citas simultáneas para el mismo doctor
- Valida que la cita esté dentro del horario laboral del doctor
- Considera la duración definida por la especialidad
- Impide crear citas fuera del rango permitido


2. sp_GetAppointmentsByDoctor.sql
   Este procedimiento obtiene las citas agendadas de un doctor.

- Filtra por DoctorId
- Retorna únicamente citas activas
- Permite visualizar el calendario del doctor


3. sp_GetDoctorAvailability.sql
   Este procedimiento obtiene la disponibilidad de horarios por día de un doctor.

Funcionalidad:

- Recibe DoctorId y Date
- Calcula la duración según la especialidad
- Evalúa el horario laboral del doctor
- Excluye citas ya ocupadas
- Retorna únicamente los espacios disponibles


DIAGRAMA DE BASE DE DATOS

Se incluye un archivo llamado:

database diagram.drawio

Aunque existen herramientas que generan diagramas automáticamente a partir de la base de datos,
se decidió incluir este archivo manualmente ya que el diseño del sistema comenzó desde la
modelación de la base de datos.

El archivo puede abrirse con:

https://app.diagrams.net/
(o software draw.io)

Este diagrama permite visualizar:

- Relaciones entre tablas
- Llaves primarias y foráneas
- Estructura general del sistema
- Flujo de entidades principales


CREACIÓN DE LA BASE DE DATOS

Existen dos formas de crear la base de datos:


OPCIÓN 1 — Usando migraciones de Entity Framework

Crear la migración:

dotnet ef migrations add InitialCreate

Aplicar la migración:

dotnet ef database update

Esto creará:

- Tablas
- Relaciones
- Índices
- Constraints

Después se deben ejecutar manualmente los Stored Procedures ubicados en la carpeta SqlServer.


OPCIÓN 2 — Restaurar desde Backup (.bak)

Se incluye un respaldo completo de la base de datos dentro de:

SqlServer/Backup/ClinicScheduling.bak


Restaurar en SQL Server (Windows)

RESTORE DATABASE ClinicScheduling
FROM DISK = 'C:\Ruta\ClinicScheduling.bak'
WITH REPLACE;


Restaurar en SQL Server Docker

Copiar el backup al contenedor:

docker cp ClinicScheduling.bak sqlserver:/var/opt/mssql/data/ClinicScheduling.bak

Restaurar la base:

docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost -U sa -P "Developer123" -C \
-Q "RESTORE DATABASE ClinicScheduling
FROM DISK = '/var/opt/mssql/data/ClinicScheduling.bak'
WITH REPLACE"
