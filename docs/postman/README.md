# Colección Postman — Banco Chillo

Colección única del grupo. **Todas** las peticiones pasan por el API Gateway, nunca directo a un microservicio.

- `{{baseUrl}}` = gateway + `/api/v1` (rutas de los microservicios).
- `{{gatewayUrl}}` = gateway sin `/api/v1` (solo para `/health` y `/health/servicios`).
Responsable: @Ronaldtipan (Issue #19).

| Archivo | Para qué sirve |
|---|---|
| `BancoChillo.postman_collection.json` | Requests y pruebas, una carpeta por microservicio |
| `local.postman_environment.json` | `baseUrl = http://localhost:5000/api/v1` (Docker en tu computadora) |
| `render.postman_environment.json` | `baseUrl = https://banco-chillo-gateway.onrender.com/api/v1` |

## 1. Importar

1. Abre Postman → **Import** → arrastra los **tres** archivos de esta carpeta.
2. Arriba a la derecha elige el entorno **Banco Chillo · local**.

## 2. Levantar el sistema

```powershell
docker compose --profile apps up -d --build
.\scripts\verificar-servicios.ps1      # los 8 deben salir [OK]
```

## 3. Ejecutar toda la colección (Collection Runner)

1. Clic derecho en la colección → **Run collection**.
2. Deja el orden como está (las carpetas están numeradas: los requests de crear guardan el id que usan los siguientes).
3. **Run**. Todo debe salir en verde. Toma la captura para el informe.

## 4. Cómo funciona el token

- La colección tiene un *pre-request script* que pide un JWT y lo guarda en la variable `token` (dura 50 min).
- Con `modoToken = desarrollo` usa `POST {{baseUrl}}/auth/token-desarrollo?rol={{rol}}`. **Solo funciona en local/Docker** (entorno Development).
- Cuando esté el login real (Issue #17, `POST {{baseUrl}}/auth/login`), en el entorno se pone `modoToken = login` y se llenan `usuarioPrueba` y `clavePrueba` con un **usuario ficticio de las semillas**. Nunca datos reales.
- Para probar con otro rol cambia la variable `rol` (por ejemplo `ROLE_CAJERO`) y borra el valor de la variable de colección `token`.

## 5. Qué prueba cada carpeta

| Carpeta | Pruebas |
|---|---|
| 00 · Gateway | `/health`, health agregado de los 7 servicios, rutas configuradas, 404 en ruta inexistente |
| 01 · Auth | Token de desarrollo + servicio disponible |
| 02 · Cajeros | CRUD completo de agencias (crea, lee, edita, desactiva) y 401 sin token |
| 03 · Cuentas · 04 · Transacciones · 05 · Créditos · 06 · Auditoría | Por ahora, que el servicio responda en el health agregado |
| 07 · Notificaciones | CRUD completo de plantillas |

## 6. Agregar tus requests (cada responsable de módulo)

Comenta en el **Issue #19** con:

```
Carpeta: 03 · Cuentas
Request: POST {{baseUrl}}/cuentas/socios
Rol necesario: ROLE_ADMIN
Body de ejemplo (datos ficticios): { ... }
Respuesta esperada: 201 y el socio creado con su id
```

O, si ya sabes usar Postman: agrega el request dentro de tu carpeta, con al menos `pm.test("201 creado", () => pm.response.to.have.status(201));`,
**Export** → Collection v2.1 → reemplaza `BancoChillo.postman_collection.json` y súbelo en tu PR.

Reglas: siempre `{{baseUrl}}` (nunca `localhost:5003` ni la URL escrita a mano), datos ficticios, sin tokens ni claves pegados en el JSON.

## 7. Errores comunes

| Ves | Causa | Solución |
|---|---|---|
| `502` | El microservicio de esa ruta está caído | `docker compose ps` y `docker compose logs <servicio>` |
| `429` | Más de 100 peticiones/min (o más de 10 logins/min) | Espera 1 minuto |
| `401` en todo | No se obtuvo token | Revisa la consola de Postman (abajo a la izquierda) y que Auth esté arriba |
| `Could not get response` | El gateway no está levantado | `docker compose --profile apps up -d` |
