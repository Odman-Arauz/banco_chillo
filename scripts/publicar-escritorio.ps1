# Genera el ejecutable de la app de escritorio del cajero (un solo .exe, no requiere .NET instalado).
# Uso (desde la raíz del repo):  .\scripts\publicar-escritorio.ps1
# Resultado: publicacion\escritorio\BancoChillo.Cajero.exe

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$salida = Join-Path $raiz 'publicacion\escritorio'

dotnet publish (Join-Path $raiz 'src\apps\escritorio-cajero\BancoChillo.Escritorio') `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $salida

Write-Host "`nListo: $salida\BancoChillo.Cajero.exe" -ForegroundColor Green
