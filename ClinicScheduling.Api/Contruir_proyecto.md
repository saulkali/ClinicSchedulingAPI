# Construcción y despliegue del proyecto (Backend API)

> Este documento describe cómo compilar, empaquetar y desplegar **ClinicScheduling API** en ambientes locales y servidor.

---

## 1) Requisitos previos

- .NET SDK 9.x
- Docker (para despliegue containerizado)
- Acceso a una instancia SQL Server
- Variables/secretos de entorno para credenciales de BD y JWT

---

## 2) Build local de la solución

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet build -c Release
```

Para ejecutar únicamente la API:

```bash
dotnet run --project ClinicScheduling.Api
```

---

## 3) Build de imagen Docker

El `Dockerfile` está dentro de `ClinicScheduling.Api/` y usa multi-stage build:

1. `mcr.microsoft.com/dotnet/sdk:9.0` para restaurar/publicar.
2. `mcr.microsoft.com/dotnet/aspnet:9.0` para runtime.

Comandos:

```bash
cd ClinicScheduling.Api
docker build -t clinicscheduling-api:latest .
```

---

## 4) Run del contenedor

```bash
docker run -d \
  --name clinicscheduling-api \
  --restart unless-stopped \
  -p 4444:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ConnectionStrings__ClinicSchedulingDb="Server=<host>,1433;Database=ClinicScheduling;User Id=sa;Password=<password>;TrustServerCertificate=True;Encrypt=False" \
  -e Jwt__Key="<jwt_key_segura>" \
  -e Jwt__Issuer="ClinicScheduling.Api" \
  -e Jwt__Audience="ClinicScheduling.Client" \
  -e Jwt__ExpireMinutes="120" \
  clinicscheduling-api:latest
```

Swagger:

- `http://localhost:4444/swagger/index.html`

---

## 5) Estrategia recomendada de configuración

Para evitar exponer datos sensibles:

- **No** dejar secretos reales en `appsettings.json`.
- Inyectar configuración por variables de entorno o gestor de secretos.
- Separar valores por ambiente (`Development`, `QA`, `Production`).

Orden de precedencia (ASP.NET Core):

1. Variables de entorno
2. appsettings.{Environment}.json
3. appsettings.json

---

## 6) CI/CD con Azure DevOps

El pipeline `azure-pipelines.yml` realiza un flujo de despliegue continuo:

1. Trigger en `main`.
2. `UseDotNet@2` para SDK 9.
3. Restore/build con `DotNetCoreCLI@2`.
4. Copia de fuentes vía `CopyFilesOverSSH@0`.
5. Construcción y despliegue del contenedor por SSH (`docker build`, `docker stop/rm`, `docker run`).

### Buenas prácticas para este pipeline

- Versionar imágenes con tag semántico además de `latest`.
- Configurar health checks del contenedor/API.
- Guardar logs del paso SSH para troubleshooting.
- Usar secretos protegidos en Azure DevOps Service Connection y variables seguras.

---

## 7) Checklist de despliegue a producción

Antes de desplegar:

- [ ] Build en Release exitoso.
- [ ] Pruebas de integración ejecutadas.
- [ ] Connection string de producción validada.
- [ ] JWT key robusta (alta entropía).
- [ ] Stored Procedures presentes en base de datos.
- [ ] Backup reciente de base de datos.

Después de desplegar:

- [ ] Verificar contenedor en estado `Up` (`docker ps`).
- [ ] Validar endpoint `/swagger/index.html`.
- [ ] Probar login y al menos 1 flujo de citas.
- [ ] Revisar logs de aplicación y sistema.

