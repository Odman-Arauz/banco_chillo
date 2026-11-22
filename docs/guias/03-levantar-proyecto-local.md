# 03 · Levantar el proyecto en tu computadora

Requisitos: haber completado las guías [01](01-instalacion-entorno.md) y [02](02-conectar-github.md),
y **Docker Desktop abierto** con *Engine running*.

## Mapa de puertos

| Servicio | URL local | Swagger / prueba |
|---|---|---|
| API Gateway | http://localhost:5000 | http://localhost:5000/health |
| Auth (.NET) | http://localhost:5001 | http://localhost:5001/swagger |
| Cuentas (.NET) | http://localhost:5003 | /swagger |
| Transacciones (.NET) | http://localhost:5004 | /swagger |
| Cajeros (.NET) ⭐ | http://localhost:5002 | /swagger |
| Créditos (.NET) | http://localhost:5005 | /swagger |
| Auditoría (.NET) | http://localhost:5006 | /swagger |
| Notificaciones (Node) ⭐ | http://localhost:5007 | /health |
| PWA (Expo web) | http://localhost:8081 | — |
| PostgreSQL | localhost:5432 | usuario `banco_admin` / clave `solo_desarrollo_local` / BD `banco_chillo_dev` |
| RabbitMQ (consola) | http://localhost:15672 | guest / guest |

---

## Opción A — Modo desarrollo (lo normal mientras programas)

Infraestructura en Docker y tu servicio con `dotnet run` / `npm run dev` (recarga rápida y depuración).

```powershell
# 0. Ponerte al día
git checkout develop
git pull origin develop

# 1. Infraestructura (Postgres, Redis, RabbitMQ)
docker compose up -d
docker compose ps                     # los 3 deben decir "healthy"

# 2. Servicio Auth (para obtener tokens de prueba) — Terminal 1
dotnet run --project src/services/auth/BancoChillo.Auth.Api

# 3. Gateway — Terminal 2
dotnet run --project src/gateway/BancoChillo.Gateway

# 4. TU microservicio — Terminal 3 (ejemplo: Cajeros)
dotnet run --project src/services/cajeros/BancoChillo.Cajeros.Api
```

Microservicio Node.js (Terminal 3, ejemplo Notificaciones):

```powershell
cd src/services/notificaciones
Copy-Item .env.example .env          # solo la primera vez
npm install                          # solo la primera vez o si cambió package.json
npm run dev
```

PWA (Terminal 4):

```powershell
cd src/apps/pwa-banca
npm install
npm run web                          # abre http://localhost:8081
```

> En VS Code puedes abrir varias terminales con **Ctrl+Shift+ñ** o con el botón `+` del panel de terminal.

## Opción B — Todo el sistema en contenedores (como en producción)

```powershell
docker compose --profile apps up -d --build
docker compose --profile apps ps
```

Para apagar: `docker compose --profile apps down`.

---

## Probar que todo funciona (5 minutos)

1. **Gateway vivo:** abre http://localhost:5000/health → `Healthy`.
2. **Listar agencias (público):** http://localhost:5000/api/v1/cajeros/agencias → JSON con AG-001 y AG-002.
3. **Obtener un token de desarrollo:**
   ```powershell
   $token = (Invoke-RestMethod -Method Post "http://localhost:5000/api/v1/auth/token-desarrollo?rol=ROLE_ADMIN").accessToken
   $token
   ```
4. **Crear una agencia con el token:**
   ```powershell
   $cuerpo = @{ codigo="AG-010"; nombre="Agencia Prueba"; direccion="Calle Ficticia 10"; ciudad="Ciudad Demo" } | ConvertTo-Json
   Invoke-RestMethod -Method Post "http://localhost:5000/api/v1/cajeros/agencias" -Headers @{Authorization="Bearer $token"} -ContentType "application/json" -Body $cuerpo
   ```
5. **En Swagger:** http://localhost:5002/swagger → botón **Authorize** → pega solo el token → prueba `POST /api/v1/cajeros/agencias`.
6. **Postman:** importa las peticiones de arriba (el responsable del Gateway publicará la colección del grupo en `docs/postman/`).

### Respuestas esperadas (memorízalas, valen para TODOS los servicios)

| Código | Cuándo |
|---|---|
| 200 / 201 / 204 | OK / creado / sin contenido (baja lógica) |
| 400 | Datos inválidos (incluye `errores` por campo) |
| 401 | Falta el token o expiró |
| 403 | Tu rol no tiene permiso |
| 404 | No existe |
| 409 | Duplicado (ej. código ya registrado) |
| 422 | Regla de negocio incumplida (ej. agencia inactiva) |

## Pruebas automáticas

```powershell
dotnet test                                          # todas las pruebas .NET
cd src/services/notificaciones; npm test; cd ../../..   # pruebas Node
cd src/apps/pwa-banca; npm test; cd ../../..            # pruebas PWA
```

## Reiniciar la base de datos desde cero

Los scripts de `database/` solo corren la **primera vez** que se crea el volumen de Postgres. Si cambiaste un `.sql`:

```powershell
docker compose down -v        # -v borra el volumen (todos los datos locales)
docker compose up -d
```

¿Algo falló? → **[04 · Errores frecuentes](04-errores-frecuentes.md)**
