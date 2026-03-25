REPOSITORIOS PÚBLICOS

El código fuente del proyecto se encuentra disponible públicamente en GitHub:

Frontend React:
https://github.com/saulkali/ClinicSchedulingFrontEnd.git

Backend API .NET:
https://github.com/saulkali/ClinicSchedulingAPI.git


INTEGRACIÓN CONTINUA Y DESPLIEGUE AUTOMÁTICO

El proyecto cuenta con pipelines configurados en Azure DevOps.  
Cualquier modificación realizada en los repositorios se refleja automáticamente en el entorno desplegado, lo que permite ahorrar tiempo de despliegue manual.

Esto incluye:

- Build automático
- Ejecución de pruebas
- Construcción de imágenes Docker
- Despliegue automático en servidor
  
- Actualización del entorno en producción

Gracias a esta configuración, no es necesario realizar deploy manual después de cada cambio.


NOTA IMPORTANTE

Se puede consultar una demo funcional desplegada en uno de mis servidores físicos. Las URLs son las siguientes:

Frontend React.js:
http://170.80.240.210:4445/

Backend API Swagger:
http://170.80.240.210:4444/swagger/index.html


CÓMO HACER DEPLOY

Dentro del proyecto encontrarás un archivo Dockerfile, el cual permite crear una imagen y desplegar la aplicación dentro de un contenedor Docker.

Comando para construir la imagen:

docker build -t clinicscheduling-api .

Comando para ejecutar el contenedor:

docker run -d -p 4444:8080 --name clinicscheduling-api clinicscheduling-api

Si también se desea desplegar el frontend, se debe construir y ejecutar su contenedor correspondiente de forma similar, exponiendo el puerto configurado para la aplicación frontend.


CONFIGURACIÓN DE LA CONNECTION STRING

La conexión a SQL Server puede configurarse de dos formas:

1. Mediante variables de entorno
2. Directamente en el archivo appsettings.json

Ejemplo de configuración en appsettings.json:

"ConnectionStrings": {
"ClinicSchedulingDb": "Server=localhost,1433;Database=ClinicScheduling;User Id=sa;Password=Developer123;TrustServerCertificate=True;Encrypt=False"
}

Nota:
Si la API se ejecuta dentro de Docker y SQL Server se encuentra en otro contenedor, normalmente no debe usarse localhost, sino el nombre del contenedor o del servicio dentro de la red Docker. Por ejemplo:

"ConnectionStrings": {
"ClinicSchedulingDb": "Server=sqlserver,1433;Database=ClinicScheduling;User Id=sa;Password=Developer123;TrustServerCertificate=True;Encrypt=False"
}


DOCUMENTACIÓN

La documentación relacionada con la base de datos y otros archivos de apoyo se encuentra dentro de la carpeta:

Docs


PRUEBAS

Para la ejecución de pruebas unitarias e integración, consultar el archivo ubicado dentro del proyecto de pruebas:

ClinicScheduling.Api.Tests/Como_ejecutar_pruebas.md
