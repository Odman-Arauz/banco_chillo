# 10 · Despliegue y CI/CD

Decisión de arquitectura: [ADR-0005](../adr/ADR-0005-estrategia-despliegue-ci-cd.md).

## Vista general

```mermaid
flowchart TB
    dev[Tu rama feat/*] -->|git push + PR| pr{Pull Request a develop}
    pr --> ci1[ci-backend.yml<br/>.NET build + xUnit]
    pr --> ci2[ci-servicios-node.yml<br/>lint + node --test + docker build]
    pr --> ci3[ci-frontend-pwa.yml<br/>lint + tsc + jest + expo export]
    pr --> ci4[gitleaks.yml<br/>secretos]
    pr --> vercelprev[Vercel: vista previa de la PWA]
    ci1 & ci2 & ci3 & ci4 -->|todo verde + 1 aprobación| develop[(develop)]
    develop -->|PR de cierre de semana| main[(main)]
    main --> cd[cd-despliegue.yml]
    cd --> ghcr[ghcr.io: imágenes Docker]
    cd --> render[Render: Deploy Hook por servicio]
    main --> vercel[Vercel: PWA producción]
    tag[tag escritorio-v*] --> cdexe[cd-escritorio.yml] --> release[GitHub Release con el .exe]
    manual[Run workflow manual] --> db[db-supabase.yml] --> supabase[(Supabase)]
```

## Pipelines del repositorio (`.github/workflows/`)

| Archivo | Se ejecuta | Qué hace |
|---|---|---|
| `ci-backend.yml` | PR/push a develop y main con cambios .NET | `dotnet restore`, `build`, `test` |
| `ci-servicios-node.yml` | Cambios en servicios Node | `npm ci`, `lint`, `test`, `docker build` |
| `ci-frontend-pwa.yml` | Cambios en la PWA | `npm ci`, `lint`, `typecheck`, `jest`, `expo export` |
| `gitleaks.yml` | Todo PR/push | Bloquea secretos filtrados |
| `cd-despliegue.yml` | Push a `main` o manual | Imagen Docker → ghcr.io → Deploy Hook de Render |
| `cd-escritorio.yml` | Cambios en la app WPF / tag `escritorio-v*` | Publica el `.exe` (artefacto o Release) |
| `db-supabase.yml` | **Manual** | Aplica `database/schema` y `database/seeds` en Supabase |

Ver el estado desde la terminal:

```powershell
gh run list --limit 10
gh run watch                # sigue en vivo la ejecución actual
gh run view --log-failed    # por qué falló
gh workflow run "CD Despliegue Microservicios" --ref main   # ejecutar a mano
```

## Construir y ejecutar imágenes Docker a mano (Parte 15 del práctico)

```powershell
# .NET (el contexto es la RAÍZ del repo porque usa building-blocks)
docker build -f src/services/cajeros/BancoChillo.Cajeros.Api/Dockerfile -t banco-chillo/cajeros .
docker run --rm -p 5002:8080 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e "ConnectionStrings__BancoChillo=Host=host.docker.internal;Port=5432;Database=banco_chillo_dev;Username=banco_admin;Password=solo_desarrollo_local" `
  banco-chillo/cajeros
# Probar: http://localhost:5002/health

# Node.js (el contexto es la carpeta del servicio)
docker build -t banco-chillo/notificaciones src/services/notificaciones
docker run --rm -p 5007:8080 --env-file src/services/notificaciones/.env `
  -e DATABASE_URL=postgres://banco_admin:solo_desarrollo_local@host.docker.internal:5432/banco_chillo_dev `
  banco-chillo/notificaciones

docker images | Select-String banco-chillo
```

## Render (microservicios)

Lo configura **una vez** el responsable de Gateway/DevOps con el docente.

1. <https://render.com> → *Sign in with GitHub* → autoriza el repositorio `banco_chillo`.
2. **New → Blueprint** → selecciona el repo → Render lee `render.yaml` y propone los servicios.
3. En el grupo de variables `banco-chillo-compartido` llena los secretos:
   `Jwt__Secreto` y `JWT_SECRETO` (el **mismo** valor, 48+ caracteres aleatorios),
   `ConnectionStrings__BancoChillo` (formato Npgsql de Supabase) y `DATABASE_URL` (URI de Supabase).
   Generar un secreto aleatorio:
   ```powershell
   [Convert]::ToBase64String((1..48 | % { Get-Random -Max 256 }) -as [byte[]])
   ```
4. En cada servicio → *Settings → Deploy Hook* → copia la URL → GitHub *Settings → Secrets → Actions*:
   `RENDER_HOOK_GATEWAY`, `RENDER_HOOK_AUTH`, `RENDER_HOOK_CAJEROS`, `RENDER_HOOK_NOTIFICACIONES`, …
5. Cuando un nuevo microservicio esté listo, su responsable agrega su bloque en `render.yaml`, la ruta en el
   Gateway (`ReverseProxy__Clusters__<servicio>__Destinations__d1__Address`) y pide crear su Deploy Hook.

Swagger queda visible en Render porque el entorno es `Demo`. URL pública: `https://banco-chillo-gateway.onrender.com`.

> Plan free: los servicios se duermen tras 15 min sin uso; la primera petición tarda unos 50 s. Antes de una
> demo, abre `/health` de cada servicio para "despertarlos".

## Vercel (PWA)

1. <https://vercel.com> → *Continue with GitHub* → **Add New → Project** → importa `banco_chillo`.
2. **Root Directory:** `src/apps/pwa-banca` (Vercel lee `vercel.json`: build `expo export`, salida `dist`).
3. **Environment Variables:** `EXPO_PUBLIC_API_URL` = `https://banco-chillo-gateway.onrender.com/api/v1`.
4. *Settings → Git → Production Branch*: `main`. Cada PR recibe una **URL de vista previa** comentada en el PR.
5. Agrega el dominio de Vercel a `Cors__Origenes__0` / `CORS_ORIGENES` en Render.

## Checklist de la Parte 15 del práctico (por microservicio)

- [ ] `Dockerfile` multi-etapa con usuario sin privilegios
- [ ] `docker build` sin errores (captura)
- [ ] `docker run` y `/health` responde 200 (captura)
- [ ] El servicio aparece en `docker compose --profile apps ps`
- [ ] CI en verde en el PR (captura de GitHub Actions)
