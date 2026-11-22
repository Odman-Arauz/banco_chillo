# 01 · Instalación del entorno (Windows 10/11)

Tiempo estimado: 45–60 minutos (la mayor parte es descarga). Hazlo **en tu computadora personal**:
en el laboratorio no se permite instalar software (normas de la práctica).

> Todos los comandos se ejecutan en **PowerShell** (botón Inicio → escribe "PowerShell").
> Los que dicen "como administrador": clic derecho → *Ejecutar como administrador*.

---

## Paso 1 — Instalar todo con `winget`

`winget` viene incluido en Windows 10/11. Copia y pega **una línea a la vez**:

```powershell
winget install --id Git.Git -e
winget install --id GitHub.cli -e
winget install --id Microsoft.DotNet.SDK.10 -e
winget install --id OpenJS.NodeJS.LTS -e
winget install --id Microsoft.VisualStudioCode -e
winget install --id Postman.Postman -e
winget install --id dbeaver.dbeaver -e
```

**Cierra y vuelve a abrir PowerShell** al terminar, para que reconozca los comandos nuevos.

> ¿`winget` no existe? Instala "App Installer" desde Microsoft Store, o descarga cada programa desde su web oficial:
> git-scm.com · cli.github.com · dotnet.microsoft.com/download · nodejs.org (versión LTS) · code.visualstudio.com

---

## Paso 2 — Docker Desktop (necesita WSL2)

1. PowerShell **como administrador**:
   ```powershell
   wsl --install
   ```
2. **Reinicia la computadora.**
3. Instala Docker:
   ```powershell
   winget install --id Docker.DockerDesktop -e
   ```
4. Abre **Docker Desktop**, acepta los términos y espera a que abajo a la izquierda diga **Engine running** (verde).
5. Comprueba:
   ```powershell
   docker run --rm hello-world
   ```
   Debe imprimir `Hello from Docker!`.

> Si Docker dice que la virtualización está desactivada: entra a la BIOS y activa **Intel VT-x / AMD-V (SVM)**.
> Ver [04-errores-frecuentes.md](04-errores-frecuentes.md#docker).

---

## Paso 3 — Extensiones de VS Code

```powershell
code --install-extension ms-dotnettools.csdevkit
code --install-extension dbaeumer.vscode-eslint
code --install-extension esbenp.prettier-vscode
code --install-extension ms-azuretools.vscode-docker
code --install-extension eamodio.gitlens
code --install-extension humao.rest-client
code --install-extension bierner.markdown-mermaid
```

Opcional (si usas IA en el editor): GitHub Copilot (gratis con GitHub Student Developer Pack) o Claude Code.

> Visual Studio 2022/2026 Community también sirve para .NET y para la app WPF de escritorio.

---

## Paso 4 — Permitir scripts de PowerShell (para `npm` y nuestros scripts)

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

Responde `S` (Sí).

---

## Paso 5 — Verificar todo

Primero clona el repositorio ([guía 02](02-conectar-github.md)) y luego, desde la carpeta del proyecto:

```powershell
.\scripts\verificar-entorno.ps1
```

Debe salir todo en verde. Ejemplo:

```
[OK]    Git -> git version 2.5x
[OK]    .NET SDK -> 10.0.xxx
[OK]    Node.js -> v22.x
[OK]    Docker Engine en ejecución
[OK]    Sesión de GitHub CLI activa
```

| Herramienta | Versión mínima | Para qué |
|---|---|---|
| Git | 2.40 | Control de versiones |
| GitHub CLI (`gh`) | 2.40 | Iniciar sesión, crear PRs, ver Issues desde la terminal |
| .NET SDK | 10.0.100 | Microservicios .NET, Gateway, app de escritorio |
| Node.js | 22 LTS | Microservicios Node/Express y la PWA |
| Docker Desktop | 27 | PostgreSQL, Redis, RabbitMQ y contenedores de los servicios |
| Postman | — | Probar APIs (Parte 13 del práctico) |
| DBeaver | — | Ver tablas de PostgreSQL / Supabase |

Siguiente: **[02 · Conectar GitHub por comando](02-conectar-github.md)**
