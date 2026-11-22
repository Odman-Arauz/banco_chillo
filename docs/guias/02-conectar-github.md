# 02 · Conectar tu cuenta de GitHub por comando

## Paso 1 — Aceptar la invitación al repositorio

Revisa tu correo o entra a <https://github.com/Odman-Arauz/banco_chillo/invitations> y pulsa **Accept invitation**.
Sin esto, no podrás hacer `push`.

## Paso 2 — Decirle a Git quién eres

Usa **el mismo correo de tu cuenta de GitHub**; si no, tus commits no aparecerán con tu foto y no cuentan como tuyos.

```powershell
git config --global user.name "Nombre Apellido"
git config --global user.email "tu-correo-de-github@ejemplo.com"
git config --global init.defaultBranch main
git config --global pull.rebase false
git config --global core.autocrlf true     # Windows: evita problemas de saltos de línea
git config --list --global                 # verificar
```

## Paso 3 — Iniciar sesión con GitHub CLI

```powershell
gh auth login
```

Responde así (flechas + Enter):

```
? Where do you use GitHub?                     GitHub.com
? What is your preferred protocol for Git?     HTTPS
? Authenticate Git with your GitHub credentials? Yes
? How would you like to authenticate?          Login with a web browser
```

Copia el código de 8 caracteres que aparece, presiona Enter, pégalo en el navegador y autoriza.
Comprueba:

```powershell
gh auth status
# ✓ Logged in to github.com account tu-usuario
```

> Con esto, `git push` ya no pide contraseña: `gh` guarda el token de forma segura en Windows.

### Alternativa: llave SSH (opcional, recomendado a futuro)

```powershell
ssh-keygen -t ed25519 -C "tu-correo-de-github@ejemplo.com"   # Enter a todo
gh ssh-key add $HOME\.ssh\id_ed25519.pub --title "Mi laptop"
ssh -T git@github.com                                          # "Hi usuario! You've successfully authenticated"
gh config set git_protocol ssh
```

> ⚠️ Nunca subas tu llave **privada** (`id_ed25519`, sin `.pub`) ni la pegues en un chat o en una IA.

## Paso 4 — Clonar el proyecto

```powershell
cd $HOME\Documents
gh repo clone Odman-Arauz/banco_chillo
cd banco_chillo
git checkout develop
code .
```

## Paso 5 — Comandos de GitHub CLI que usarás a diario

```powershell
gh issue list --assignee "@me"          # mis Issues
gh issue view 12                        # leer el Issue 12 completo
gh issue develop 12 --checkout --base develop --name feat/12-mi-tarea   # crea la rama ligada al Issue
gh pr create --base develop --fill      # abrir Pull Request
gh pr status                            # estado de mis PRs
gh pr checks                            # ver si pasó la CI
gh run list --limit 5                   # últimas ejecuciones de GitHub Actions
gh run view --log-failed                # ver por qué falló la CI
```

Siguiente: **[03 · Levantar el proyecto en local](03-levantar-proyecto-local.md)**
