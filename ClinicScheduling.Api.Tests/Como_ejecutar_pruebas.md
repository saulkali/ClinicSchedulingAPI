PRUEBAS E INTEGRACIÓN

Se integraron pruebas para cubrir la mayor parte posible del CRUD general de todos los controllers del sistema.
Estas pruebas abarcan:

- Validaciones de reglas de negocio
- Operaciones CRUD (Get, GetById, Create, Update, Delete)
- Flujo completo de los endpoints
- Validación del comportamiento de los Stored Procedures
- Escenarios de error y casos límite

Las pruebas no son unitarias aisladas, sino pruebas de integración.
Esto significa que simulan peticiones HTTP reales contra la API, permitiendo validar el flujo completo del sistema,
desde los controllers hasta la base de datos.

CONFIGURACIÓN PARA EJECUTAR LAS PRUEBAS

Dentro del proyecto de pruebas, en la carpeta:

Integrations

se encuentra la clase:

ClinicWebApplicationFactory.cs

Esta clase incluye:

- Configuración del host de pruebas
- Creación de una base de datos temporal para testing
- Carga automática de datos semilla
- Ejecución automática de Stored Procedures
- Configuración del entorno de integración

IMPORTANTE

Para ejecutar las pruebas es necesario contar con una instancia de SQL Server disponible, ya sea:

- SQL Server instalado localmente
- SQL Server ejecutándose en Docker

Dentro de la clase ClinicWebApplicationFactory.cs se encuentra la connection string utilizada para las pruebas,
aproximadamente en las líneas 39-40:

var serverConnectionString =
"Server=localhost,1433;User Id=sa;Password=Developer123;TrustServerCertificate=True;MultipleActiveResultSets=True;";

Esta conexión se utiliza para:

- Crear automáticamente una base de datos de pruebas
- Ejecutar migraciones
- Cargar los Stored Procedures
- Insertar datos semilla
- Ejecutar las pruebas de integración

Las pruebas ejecutan peticiones HTTP reales, permitiendo validar:

- Funcionamiento de controllers
- Reglas de negocio
- Flujo completo del sistema
- Integración con base de datos
- Correcta ejecución de Stored Procedures

COBERTURA DE PRUEBAS

El proyecto cuenta con más de 40 pruebas que validan múltiples funcionalidades del sistema, incluyendo:

- Creación de doctores
- Creación de pacientes
- Creación de citas
- Validación de horarios disponibles
- Prevención de citas duplicadas
- Validación de horarios laborales
- Actualización de entidades
- Eliminación de registros
- Consulta de disponibilidad
- Integración con Stored Procedures

Estas pruebas sirvieron como apoyo para validar el correcto funcionamiento del sistema y detectar posibles errores
durante el desarrollo. Aunque aún pueden existir algunos bugs menores, las pruebas permiten asegurar que el flujo
principal del sistema funcione correctamente.