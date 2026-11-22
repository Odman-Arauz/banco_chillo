using System.Collections.ObjectModel;
using System.Net.Http;
using BancoChillo.Escritorio.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BancoChillo.Escritorio.VistasModelo;

/// <summary>
/// Patrón MVVM: la Vista (XAML) solo se enlaza (Binding) a propiedades y comandos de esta clase.
/// [ObservableProperty] genera la propiedad pública y avisa a la vista cuando cambia.
/// [RelayCommand] genera un ICommand para los botones.
/// </summary>
public sealed partial class PrincipalVistaModelo : ObservableObject
{
    private readonly ClienteBancoChillo _cliente = new();

    [ObservableProperty]
    private string _urlGateway =
        Environment.GetEnvironmentVariable("BANCO_CHILLO_GATEWAY") ?? "http://localhost:5000";

    [ObservableProperty]
    private string _rolSeleccionado = "ROLE_CAJERO";

    [ObservableProperty]
    private string _mensaje = "Presione «Conectar» para verificar el API Gateway.";

    [ObservableProperty]
    private bool _ocupado;

    public IReadOnlyList<string> Roles { get; } =
        ["ROLE_CAJERO", "ROLE_SUPERVISOR_CAJA", "ROLE_ADMIN"];

    public ObservableCollection<AgenciaDto> Agencias { get; } = [];

    [RelayCommand]
    private async Task ConectarAsync()
    {
        await EjecutarAsync(async () =>
        {
            _cliente.ConfigurarGateway(UrlGateway);
            var enLinea = await _cliente.VerificarGatewayAsync();
            if (!enLinea)
            {
                Mensaje = "Sin conexión con el API Gateway. ¿Está corriendo? (docs/guias/03-levantar-proyecto-local.md)";
                return;
            }

            await _cliente.ObtenerTokenDesarrolloAsync(RolSeleccionado);
            Mensaje = $"Conectado como {RolSeleccionado}.";
            await CargarAgenciasAsync();
        });
    }

    [RelayCommand]
    private Task RefrescarAsync() => EjecutarAsync(CargarAgenciasAsync);

    private async Task CargarAgenciasAsync()
    {
        var agencias = await _cliente.ListarAgenciasAsync();
        Agencias.Clear();
        foreach (var agencia in agencias)
        {
            Agencias.Add(agencia);
        }

        Mensaje = $"{Agencias.Count} agencia(s) cargada(s) desde el microservicio Cajeros.";
    }

    private async Task EjecutarAsync(Func<Task> accion)
    {
        Ocupado = true;
        try
        {
            await accion();
        }
        catch (HttpRequestException ex)
        {
            Mensaje = $"Error HTTP: {ex.StatusCode?.ToString() ?? ex.Message}";
        }
        catch (Exception ex) when (ex is UriFormatException or InvalidOperationException or TaskCanceledException)
        {
            Mensaje = $"Error: {ex.Message}";
        }
        finally
        {
            Ocupado = false;
        }
    }
}
