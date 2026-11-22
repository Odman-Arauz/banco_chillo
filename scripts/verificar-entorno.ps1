# =====================================================================
# Verifica que tu computadora tenga todo lo necesario para Banco Chillo.
# Uso (PowerShell, desde la raíz del repo):  .\scripts\verificar-entorno.ps1
# Si PowerShell bloquea el script:  Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
# =====================================================================

$errores = 0

function Probar($nombre, $comando, $minimo, $comoInstalar) {
    $version = $null
    $global:LASTEXITCODE = 0
    try {
        $salida = & $comando[0] $comando[1..($comando.Length - 1)] 2>$null
        $version = @($salida)[0]
    } catch { }
    if ($LASTEXITCODE -ne 0 -or -not $version) {
        Write-Host "[FALTA] $nombre" -ForegroundColor Red
        Write-Host "        Instalar: $comoInstalar" -ForegroundColor Yellow
        $script:errores++
    } else {
        Write-Host "[OK]    $nombre -> $version (mínimo recomendado: $minimo)" -ForegroundColor Green
    }
}

Write-Host "`n=== Herramientas ===" -ForegroundColor Cyan
Probar 'Git'            @('git', '--version')        '2.40'   'winget install --id Git.Git -e'
Probar 'GitHub CLI'     @('gh', '--version')         '2.40'   'winget install --id GitHub.cli -e'
Probar '.NET SDK'       @('dotnet', '--version')     '10.0.100' 'winget install --id Microsoft.DotNet.SDK.10 -e'
Probar 'Node.js'        @('node', '--version')       'v22'    'winget install --id OpenJS.NodeJS.LTS -e'
Probar 'npm'            @('npm', '--version')        '10'     '(viene con Node.js)'
Probar 'Docker'         @('docker', '--version')     '27'     'winget install --id Docker.DockerDesktop -e  (requiere WSL2: wsl --install)'

Write-Host "`n=== Servicios ===" -ForegroundColor Cyan
docker info --format '{{.ServerVersion}}' *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Docker está instalado pero NO está corriendo. Abre Docker Desktop y espera a que diga 'Engine running'." -ForegroundColor Red
    $errores++
} else {
    Write-Host "[OK]    Docker Engine en ejecución" -ForegroundColor Green
}

gh auth status *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] GitHub CLI sin sesión. Ejecuta: gh auth login  (ver docs/guias/02-conectar-github.md)" -ForegroundColor Red
    $errores++
} else {
    Write-Host "[OK]    Sesión de GitHub CLI activa" -ForegroundColor Green
}

$nombreGit = git config --global user.name
$correoGit = git config --global user.email
if (-not $nombreGit -or -not $correoGit) {
    Write-Host "[ERROR] Falta tu identidad de Git: git config --global user.name 'Tu Nombre'; git config --global user.email 'tu@correo'" -ForegroundColor Red
    $errores++
} else {
    Write-Host "[OK]    Git configurado como $nombreGit <$correoGit>" -ForegroundColor Green
}

Write-Host "`n=== Puertos usados por el proyecto ===" -ForegroundColor Cyan
foreach ($puerto in 5432, 6379, 5672, 5000, 5001, 5002, 5007, 8081) {
    $ocupado = Get-NetTCPConnection -LocalPort $puerto -State Listen -ErrorAction SilentlyContinue
    if ($ocupado) {
        $proceso = (Get-Process -Id $ocupado[0].OwningProcess -ErrorAction SilentlyContinue).ProcessName
        Write-Host "[INFO]  Puerto $puerto en uso por '$proceso' (normal si es Docker o el proyecto ya está corriendo)" -ForegroundColor DarkYellow
    } else {
        Write-Host "[OK]    Puerto $puerto libre" -ForegroundColor Green
    }
}

Write-Host ""
if ($errores -eq 0) {
    Write-Host "Todo listo. Sigue con docs/guias/03-levantar-proyecto-local.md" -ForegroundColor Green
} else {
    Write-Host "Hay $errores problema(s). Revisa docs/guias/01-instalacion-entorno.md y docs/guias/04-errores-frecuentes.md" -ForegroundColor Red
    exit 1
}
