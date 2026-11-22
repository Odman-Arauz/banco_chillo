# 12 · Informe de la Práctica Experimental N° 1 (PDF)

Práctica: *Desarrollo e implementación de microservicios y APIs REST seguras mediante contenedores Docker y
prácticas de integración y despliegue continuo (CI/CD)* — Sistemas Web, 4.º nivel, Desarrollo de Software.

## Formato (indicaciones de entrega)

- Informe **institucional en PDF**: sello y nombre del instituto; carrera, nivel, asignatura; **apellido y nombre**.
- Enunciado de la actividad.
- **Capturas de pantalla de la resolución y del resultado** en cada parte (con tu usuario de GitHub o tu nombre visible cuando sea posible).
- Se sube a la plataforma en el apartado de la actividad.

Cada estudiante entrega **su** informe con **sus** evidencias (sus commits, sus PRs, su microservicio),
más la sección grupal común.

## Qué evidenciar en cada parte, en Banco Chillo

| Parte | Enunciado del práctico | Evidencia en Banco Chillo |
|---|---|---|
| 1 | Configuración del entorno | Captura de `.\scripts\verificar-entorno.ps1` en verde y `gh auth status` |
| 2 | Crear proyecto Node.js e instalar dependencias | Responsable de Notificaciones: `npm init`/`npm install` de su servicio. Resto del equipo: levantar Notificaciones en su PC (`npm install`, `npm run dev`) explicando su `package.json`, más `dotnet restore` de su propio servicio |
| 3 | Estructura de directorios y archivo inicial | Árbol de carpetas (`tree /F src\services\<tuyo>`) y explicación de capas |
| 4 | Diseñar la API (endpoints CRUD) | Tabla de endpoints de tu spec + mockups de Stitch relacionados |
| 5 | Modelo de datos y base de datos | Tu script SQL, diagrama ER y `\dt <esquema>.*` en psql/DBeaver |
| 6 | Crear (POST) con validación | Postman/Swagger: 201 válido y 400 inválido |
| 7 | Leer (GET todos y por id) | Postman: 200 lista, 200 por id, 404 inexistente |
| 8 | Actualizar (PUT) | Postman: 200 y 404 |
| 9 | Eliminar (DELETE) | Postman: 204 (baja lógica) y verificación en la BD |
| 10 | Independencia de microservicios | Detener un servicio (`docker stop`) y mostrar que los otros siguen respondiendo |
| 11 | Proyecto de API REST con Express | Microservicio Notificaciones (Node + Express) corriendo y consumido por el Gateway con el JWT de .NET |
| 12 | Arquitectura del proyecto | Diagrama (C4 o Mermaid) de gateway + microservicios + BD |
| 13 | Desarrollo de la API REST | Colección Postman del grupo ejecutada (*Runner*) |
| 14 | Seguridad: JWT, autorización, validación | 401 sin token, 403 con rol incorrecto, 400 con datos inválidos; `helmet`/CORS/rate limit |
| 15 | Imagen Docker | `docker build`, `docker run`, `docker ps`, `/health` 200 |
| 16 | Documento completo | Este PDF + enlace al repositorio, a los PRs y al despliegue |

**CI/CD (transversal):** captura de GitHub Actions en verde en tu PR, del despliegue en Render/Vercel y del Release del `.exe`.

## Entrega semanal parcial

Cada semana se agregan las partes trabajadas. Semana 5: **Partes 1, 3, 4 y 5** (entorno, estructura, diseño de API +
mockups, base de datos). Ver [docs/planificacion/semana-05.md](../planificacion/semana-05.md).

## Consejos para las capturas

- Usa **Win + Shift + S** y recorta solo lo relevante.
- En Postman, muestra método, URL, cuerpo, **código de estado** y respuesta en la misma captura.
- Pon un pie de figura: *"Figura 5. POST /api/v1/socios devuelve 201 Created"*.
