# 11 · App de escritorio del cajero (.exe)

Decisión: [ADR-0006](../adr/ADR-0006-app-escritorio-wpf.md). Proyecto: `src/apps/escritorio-cajero/BancoChillo.Escritorio` (WPF, .NET 10, MVVM).

## Ejecutar en desarrollo

```powershell
# Necesita el Gateway, Auth y Cajeros corriendo (guía 03)
dotnet run --project src/apps/escritorio-cajero/BancoChillo.Escritorio
```

En la ventana: deja `http://localhost:5000`, elige un rol, pulsa **Conectar** → debe listar las agencias.

Para apuntar a otro gateway (por ejemplo, Render):

```powershell
$env:BANCO_CHILLO_GATEWAY = "https://banco-chillo-gateway.onrender.com"
dotnet run --project src/apps/escritorio-cajero/BancoChillo.Escritorio
```

## Generar el .exe

```powershell
.\scripts\publicar-escritorio.ps1
# → publicacion\escritorio\BancoChillo.Cajero.exe  (~130 MB, no necesita .NET instalado)
```

Publicar una versión descargable para todos (GitHub Release):

```powershell
git checkout main
git pull
git tag escritorio-v0.1.0
git push origin escritorio-v0.1.0
# GitHub Actions → "CD App de Escritorio (.exe)" → Releases → BancoChillo.Cajero.exe
```

> Windows SmartScreen puede advertir "aplicación no reconocida" porque el .exe no está firmado:
> *Más información → Ejecutar de todas formas*. Es normal en un proyecto académico.

## Estructura MVVM

```
BancoChillo.Escritorio/
├── App.xaml                         # Recursos globales (convertidores, estilos)
├── MainWindow.xaml                  # Vista: SOLO diseño y Bindings
├── MainWindow.xaml.cs               # Vacío a propósito
├── VistasModelo/
│   └── PrincipalVistaModelo.cs      # Estado + comandos ([ObservableProperty], [RelayCommand])
└── Servicios/
    └── ClienteBancoChillo.cs        # HttpClient hacia el Gateway (JWT)
```

## Cómo agregar una pantalla (ejemplo: Apertura de caja)

1. `Servicios/ClienteBancoChillo.cs`: método `AbrirCajaAsync(...)` que llama a `POST /api/v1/cajas/apertura`.
2. `VistasModelo/AperturaCajaVistaModelo.cs`: propiedades de cantidades por denominación, total calculado y `[RelayCommand] AbrirCajaAsync`.
3. `Vistas/AperturaCajaVista.xaml`: `UserControl` con la tabla de denominaciones enlazada al VistaModelo.
4. Navegación: en `MainWindow` un `TabControl` o un `ContentControl` que muestre la vista activa.
5. Reglas de negocio: **siempre en el microservicio**, nunca solo en la app de escritorio.

Atajos de teclado recomendados (igual que la PWA): `F2` depósito, `F3` retiro, `F9` estado de caja
(`<Window.InputBindings><KeyBinding Key="F2" Command="{Binding IrADepositoCommand}"/></Window.InputBindings>`).
