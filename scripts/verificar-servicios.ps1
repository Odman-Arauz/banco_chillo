# =====================================================================
# Verifica que el gateway y los 7 microservicios respondan en /health.
# Uso (PowerShell, desde la raíz del repo), con el stack levantado:
#   docker compose --profile apps up -d --build
#   .\scripts\verificar-servicios.ps1
# Para revisar otra máquina o puertos:  .\scripts\verificar-servicios.ps1 -Servidor 192.168.1.20
# Si PowerShell bloquea el script:  Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
# =====================================================================

param(
    [string]$Servidor = 'localhost',
    [int]$TimeoutSegundos = 5
)

$servicios = [ordered]@{
    'Gateway'        = 5000
    'Auth'           = 5001
    'Cajeros'        = 5002
    'Cuentas'        = 5003
    'Transacciones'  = 5004
    'Creditos'       = 5005
    'Auditoria'      = 5006
    'Notificaciones' = 5007
}

$fallas = 0
Write-Host "`n=== Salud de los servicios en $Servidor ===" -ForegroundColor Cyan

foreach ($nombre in $servicios.Keys) {
    $puerto = $servicios[$nombre]
    $url = "http://${Servidor}:${puerto}/health"
    $cronometro = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $respuesta = Invoke-WebRequest -Uri $url -TimeoutSec $TimeoutSegundos -UseBasicParsing -ErrorAction Stop
        $ms = $cronometro.ElapsedMilliseconds
        Write-Host ("[OK]    {0,-15} :{1}  {2} en {3} ms" -f $nombre, $puerto, $respuesta.StatusCode, $ms) -ForegroundColor Green
    } catch {
        $fallas++
        $codigo = $null
        if ($_.Exception.Response) { $codigo = [int]$_.Exception.Response.StatusCode }
        $detalle = if ($codigo) { "respondió $codigo" } else { 'no responde' }
        Write-Host ("[FALLA] {0,-15} :{1}  {2}" -f $nombre, $puerto, $detalle) -ForegroundColor Red
    }
}

# Resumen del gateway: así ve él a cada microservicio (misma red de Docker).
# Formato: { "gateway": "Healthy", "servicios": { "auth": "Healthy", "cajeros": "Unhealthy", ... } }
Write-Host "`n=== Vista del gateway (/health/servicios) ===" -ForegroundColor Cyan
try {
    $reporte = Invoke-RestMethod -Uri "http://${Servidor}:5000/health/servicios" -TimeoutSec 10 -ErrorAction Stop
    Write-Host ("        {0,-15} {1}" -f 'gateway', $reporte.gateway) -ForegroundColor Green
    foreach ($servicio in $reporte.servicios.PSObject.Properties) {
        $color = if ($servicio.Value -eq 'Healthy') { 'Green' } else { 'Red' }
        Write-Host ("        {0,-15} {1}" -f $servicio.Name, $servicio.Value) -ForegroundColor $color
    }
} catch {
    Write-Host '        El gateway no respondió /health/servicios.' -ForegroundColor Yellow
}

Write-Host ''
if ($fallas -eq 0) {
    Write-Host 'Todo en verde: los 8 servicios responden. Toma la captura para el informe.' -ForegroundColor Green
    exit 0
}

Write-Host "$fallas servicio(s) con FALLA. Revisa: docker compose ps   y   docker compose logs <servicio>" -ForegroundColor Red
exit 1
