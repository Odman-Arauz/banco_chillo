# 04 · Errores frecuentes y cómo resolverlos

Busca el mensaje de error con **Ctrl+F**. Si no está aquí:
1. Lee el error completo (la causa real suele estar en la **última** línea roja).
2. Pégalo a tu IA **junto con** el comando que ejecutaste y el archivo involucrado ([guía 07](07-uso-de-ia.md)).
3. Si sigues bloqueado más de 30 minutos, coméntalo en tu Issue con captura y el texto del error.
4. Cuando lo resuelvas, **agrega el error y la solución a esta guía** (PR `docs: ...`).

---

## Instalación y terminal

| Error | Causa | Solución |
|---|---|---|
| `'git' / 'dotnet' / 'node' / 'gh' no se reconoce como nombre de un cmdlet` | No está instalado o la terminal se abrió antes de instalar | Cierra **todas** las terminales y VS Code, vuelve a abrir. Si persiste, reinstala con `winget` ([guía 01](01-instalacion-entorno.md)) |
| `npm.ps1 no se puede cargar porque la ejecución de scripts está deshabilitada` | Política de PowerShell | `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` |
| `winget no se reconoce` | Falta App Installer | Instálalo desde Microsoft Store o descarga los programas desde su web |
| Rutas con espacios o tildes fallan (`C:\Users\José Pérez\...`) | Algunas herramientas no soportan esos caracteres | Clona el repo en una ruta simple: `C:\dev\banco_chillo` |

## .NET

| Error | Causa | Solución |
|---|---|---|
| `A compatible .NET SDK was not found` / `Requested SDK version: 10.0.100` | No tienes .NET 10 (el repo lo exige en `global.json`) | `winget install --id Microsoft.DotNet.SDK.10 -e` y reabre la terminal. Verifica con `dotnet --list-sdks` |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0` | Tienes .NET 8/9 activo | Igual que arriba |
| `error NU1101: Unable to find package` / `NU1301` | Sin internet o fuente NuGet caída | `dotnet nuget locals all --clear` y `dotnet restore` |
| `Falta Jwt:Secreto (mínimo 32 caracteres)` al arrancar | Falta `appsettings.Development.json` o la variable `ASPNETCORE_ENVIRONMENT` no es Development | Ejecuta con `dotnet run --project ...` (usa `launchSettings.json`) o define `$env:ASPNETCORE_ENVIRONMENT="Development"` |
| `Falta ConnectionStrings:BancoChillo` | Igual que arriba | Igual que arriba |
| `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432` | Postgres no está corriendo | `docker compose up -d` y espera a `healthy` |
| `28P01: password authentication failed for user "banco_admin"` | Tienes **otro PostgreSQL instalado en Windows** usando el puerto 5432, o un volumen viejo con otra contraseña | Ver "Puerto 5432 ocupado" abajo; o `docker compose down -v` y `docker compose up -d` |
| `42P01: relation "cajeros_bc.xxx" does not exist` | Tu tabla nueva no se creó: los scripts solo corren con un volumen nuevo | `docker compose down -v` y `docker compose up -d`. Revisa `docker logs banco_chillo_postgres` |
| `42703: column xxx does not exist` | La propiedad C# no coincide con la columna SQL | Con `UseSnakeCaseNamingConvention`, `FechaApertura` ↔ `fecha_apertura`. Corrige el nombre o mapea con `.HasColumnName("...")` |
| `Failed to bind to address http://localhost:5002: address already in use` | Ya hay una instancia corriendo | Cierra la otra terminal, o: `Get-NetTCPConnection -LocalPort 5002 \| % { Stop-Process -Id $_.OwningProcess }` |
| `Unable to resolve service for type 'IXxxRepositorio'` | No registraste el repositorio en `DependencyInjection.cs` | `services.AddScoped<IXxxRepositorio, XxxRepositorio>();` |
| `No handler was found for request of type ...` | El handler no está en el proyecto Application registrado | Verifica que el handler esté en `BancoChillo.<Modulo>.Application` y que llamas a `AgregarAplicacion<Modulo>()` en `Program.cs` |
| Swagger responde **401** | Falta el token | Botón **Authorize**, pega solo el token (sin la palabra `Bearer`) |
| Swagger responde **403** | El token es de un rol sin permiso | Pide el token con el rol correcto: `?rol=ROLE_ADMIN` |
| `IDX10223: Lifetime validation failed. The token is expired` | El token caducó (60 min en desarrollo) | Pide uno nuevo |

## Node.js

| Error | Causa | Solución |
|---|---|---|
| `Falta la variable de entorno DATABASE_URL` | No creaste el `.env` | `Copy-Item .env.example .env` dentro de la carpeta del servicio |
| `Cannot find module 'express'` | No instalaste dependencias | `npm install` en la carpeta del servicio |
| `npm ci` falla con `package-lock.json` desincronizado | Agregaste paquetes sin actualizar el lock | `npm install` y **sube también** `package-lock.json` |
| `ERR_MODULE_NOT_FOUND` con una ruta propia | En ESM las rutas relativas necesitan extensión | `import x from './archivo.js'` (con `.js`) |
| `EADDRINUSE: address already in use :::5007` | Otra instancia corriendo | Cierra la otra terminal o cambia `PUERTO` en `.env` |
| `401 Token inválido` con un token que sí funciona en .NET | `JWT_SECRETO` del `.env` distinto a `Jwt:Secreto` de .NET | Copia el mismo valor que está en `appsettings.Development.json` |

## PWA (Expo)

| Error | Causa | Solución |
|---|---|---|
| `expo: command not found` | Dependencias no instaladas | `npm install` dentro de `src/apps/pwa-banca` |
| Pantalla en blanco en el navegador | Error de JavaScript | F12 → pestaña *Console* |
| `Access to fetch ... has been blocked by CORS policy` | El origen de la PWA no está permitido | Agrega el origen en `Cors:Origenes` del servicio/gateway (local ya incluye `http://localhost:8081`) |
| "Sin conexión con el API Gateway" | El gateway no está corriendo | `dotnet run --project src/gateway/BancoChillo.Gateway` |
| Cambios que no aparecen | Caché de Metro o del Service Worker | `npx expo start --web --clear`; en el navegador F12 → Application → Service Workers → *Unregister* |

## Docker

| Error | Causa | Solución |
|---|---|---|
| `failed to connect to the docker API at npipe:////./pipe/dockerDesktopLinuxEngine` | Docker Desktop cerrado | Ábrelo y espera *Engine running* |
| `WSL 2 installation is incomplete` / `WSL kernel` | Falta WSL2 | `wsl --install` como administrador y reinicia; luego `wsl --update` |
| `Virtualization support not detected` | Virtualización apagada en BIOS | Activa Intel VT-x / AMD SVM en la BIOS |
| `Bind for 0.0.0.0:5432 failed: port is already allocated` / puerto 5432 ocupado | Tienes PostgreSQL instalado en Windows | Detén el servicio: `Stop-Service postgresql*` (como administrador) o desinstálalo; alternativa: cambia el puerto en `docker-compose.yml` a `"5433:5432"` **solo en tu máquina** (no lo subas) |
| `docker-entrypoint-initdb.d/00-ejecutar-scripts.sh: /bin/bash^M: bad interpreter` | El `.sh` se guardó con finales de línea de Windows (CRLF) | El repo ya fuerza LF (`.gitattributes`). Si lo editaste: en VS Code abajo a la derecha cambia `CRLF` → `LF` y guarda |
| `ERROR: ... syntax error at or near` en `docker logs banco_chillo_postgres` | Tu `.sql` tiene un error | Corrige el script y `docker compose down -v; docker compose up -d` |
| La compilación de la imagen tarda mucho o se queda sin espacio | Caché de Docker llena | `docker system prune -f` |

## Git y GitHub

| Error | Causa | Solución |
|---|---|---|
| `Permission denied (publickey)` | SSH sin llave registrada | Usa HTTPS con `gh auth login`, o registra tu llave ([guía 02](02-conectar-github.md)) |
| `remote: Permission to Odman-Arauz/banco_chillo.git denied` / `403` | No aceptaste la invitación o tu sesión es de otra cuenta | Acepta la invitación; `gh auth status`; `gh auth login` de nuevo |
| `! [remote rejected] develop -> develop (protected branch hook declined)` | Intentaste hacer push directo a `develop` o `main` | **Correcto que falle.** Crea una rama `feat/...` y abre un Pull Request ([guía 05](05-flujo-git-commits-pr.md)) |
| `Updates were rejected because the remote contains work that you do not have` | Alguien subió antes a tu rama | `git pull origin <tu-rama>` y vuelve a hacer push |
| `CONFLICT (content): Merge conflict in archivo` | Tú y otra persona cambiaron las mismas líneas | Abre el archivo en VS Code → *Resolve in Merge Editor* → elige/combina → `git add archivo` → `git commit` |
| `Your local changes would be overwritten by checkout` | Cambios sin guardar al cambiar de rama | `git stash` → cambias de rama → `git stash pop` |
| Subí un secreto por error | — | **Avisa de inmediato** al Tech Lead. Borrarlo en un commit nuevo NO basta (queda en el historial): hay que rotar la clave |
| Hice commit en `develop` local por error | — | `git branch feat/XX-mi-tarea` (guarda tu trabajo) → `git reset --hard origin/develop` → `git checkout feat/XX-mi-tarea` |
| Gitleaks falla en el PR | Detectó algo parecido a un secreto | Revisa el log del job; si es un dato ficticio, usa prefijo `test_`/`mock_` o pide ajustar `.gitleaks.toml` |
