# Guía rápida: `docker-compose.yml`

Este repositorio ahora incluye un archivo `docker-compose.yml` en la raíz para levantar un entorno base de **backend + SQL Server**.

> ⚠️ Nota importante: este compose **no fue probado se adjunta como ejemplo**

---

## 1) Qué incluye el compose

- `sqlserver`: SQL Server 2022 con volumen persistente.
- `backend`: API `.NET 9` del proyecto `ClinicScheduling.Api`.
- Red dedicada `clinicscheduling-network`.
- Healthcheck de SQL Server y arranque ordenado (`depends_on` con `service_healthy`).

---

## 2) Variables de entorno recomendadas

Antes de levantar, define al menos:

```bash
export SA_PASSWORD='TuPasswordSegura_123!'
export JWT_KEY='una_clave_jwt_larga_y_segura'
```

También puedes usar un archivo `.env` en la raíz del repo con esas variables.

---

## 3) Levantar servicios

Desde la raíz del repositorio:

```bash
docker compose up -d --build
```

Comandos útiles:

```bash
docker compose ps
docker compose logs -f backend
docker compose down
```

Si quieres eliminar también el volumen de SQL Server:

```bash
docker compose down -v
```

---

## 4) Endpoints esperados

Con el compose levantado:

- API Swagger: `http://localhost:4444/swagger/index.html`
- SQL Server expuesto en: `localhost:1433`

---

## 5) Sobre el frontend

El frontend está en **otro repositorio** (`ClinicSchedulingFrontEnd`) y por eso **no se incluyó** un servicio `frontend` funcional en este compose dentro de este repo.

Si deseas correr los 3 servicios (SQL + API + frontend), puedes:

1. Clonar el repo frontend en una carpeta vecina,
2. agregar un servicio `frontend` al compose apuntando al `Dockerfile` de ese repo,
3. configurar `VITE_API_URL` para que apunte al backend.

Ejemplo esperado de variable para frontend:

```env
VITE_API_URL=http://backend:8080
```

---
