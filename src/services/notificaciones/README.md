# Microservicio Notificaciones (Node.js + Express)

Es el **microservicio Node.js** de Banco Chillo (ADR-0004) y el ejemplo de referencia para Node. Corresponde a las Partes 2 a 15 del práctico:
proyecto Node, estructura de carpetas, CRUD, seguridad con JWT y la imagen Docker.

## Ejecutar en local

```bash
# 1. Base de datos (desde la raíz del repo)
docker compose up -d postgres

# 2. Este servicio
cd src/services/notificaciones
cp .env.example .env        # En PowerShell: Copy-Item .env.example .env
npm install
npm run dev                 # http://localhost:5007/health
npm test                    # pruebas (no necesitan base de datos)
npm run lint
```

Para obtener un token de prueba, levanta el servicio Auth (`dotnet run --project src/services/auth/BancoChillo.Auth.Api`) y ejecuta:

```bash
curl -X POST "http://localhost:5001/api/v1/auth/token-desarrollo?rol=ROLE_ADMIN"
```

## Estructura (cópiala para cada módulo nuevo: preferencias, suscripciones, envíos)

```
src/
├── server.js                  # Arranca el servidor (único archivo que abre conexiones reales)
├── app.js                     # Construye Express: middlewares, rutas y errores (se prueba sin BD)
├── config/entorno.js          # Lee y valida variables de entorno
├── db/pool.js                 # Pool de PostgreSQL (local o Supabase)
├── middlewares/
│   ├── autenticacion.js       # autenticar (JWT) y autorizar (roles)
│   └── errores.js             # ErrorHttp + respuesta ProblemDetails (igual que .NET)
└── modulos/plantillas/        # Un módulo por recurso
    ├── plantillas.esquemas.js     # Validación con zod
    ├── plantillas.repositorio.js  # SQL parametrizado
    └── plantillas.rutas.js        # Endpoints REST
test/plantillas.test.js        # node --test + supertest con repositorio en memoria
```

## Endpoints

| Método | Ruta | Rol |
|---|---|---|
| GET | `/health` | público |
| GET | `/api/v1/notificaciones/plantillas` | cualquier usuario autenticado |
| GET | `/api/v1/notificaciones/plantillas/:id` | cualquier usuario autenticado |
| POST | `/api/v1/notificaciones/plantillas` | ROLE_ADMIN, ROLE_SUPERADMIN |
| PUT | `/api/v1/notificaciones/plantillas/:id` | ROLE_ADMIN, ROLE_SUPERADMIN |
| DELETE | `/api/v1/notificaciones/plantillas/:id` (baja lógica) | ROLE_ADMIN, ROLE_SUPERADMIN |
