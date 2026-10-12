using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using AlertaTemprana.Services;
using Java.Util;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace AlertaTemprana.Platforms.Android.Services;

public class BluetoothService : IWaterLevelSource
{
    private const string ClaveDireccion = "BT_DIRECCION_MODULO";

    private static readonly UUID SppUuid =
        UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

    private BluetoothSocket? _socket;
    private System.IO.Stream? _inputStream;
    private CancellationTokenSource? _cts;

    // Motivo del último resultado, para mostrarlo al usuario.
    public string UltimoMensaje { get; private set; } = "Sin intentos todavía.";

    // Nombre del módulo actualmente conectado (null si no hay conexión).
    public string? NombreConectado { get; private set; }

    public event EventHandler<string>? DataReceived;
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>Pide el permiso BLUETOOTH_CONNECT (Android 12+).</summary>
    public async Task<bool> PedirPermisoAsync()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return true;

        var estado = await Permissions.RequestAsync<BluetoothConnectPermission>();
        if (estado == PermissionStatus.Granted)
            return true;

        Fallar($"Permiso de Bluetooth no otorgado ({estado}). " +
               "Actívalo en Ajustes → Apps → AlertaTemprana → Permisos.");
        return false;
    }

    /// <summary>Lista los dispositivos emparejados con su nombre y dirección MAC.</summary>
    public List<(string Nombre, string Direccion)> ObtenerEmparejados()
    {
        var lista = new List<(string Nombre, string Direccion)>();

        var adapter = BluetoothAdapter.DefaultAdapter;
        if (adapter is null)
        {
            UltimoMensaje = "Este dispositivo no tiene adaptador Bluetooth.";
            return lista;
        }
        if (!adapter.IsEnabled)
        {
            UltimoMensaje = "El Bluetooth del tablet está apagado.";
            return lista;
        }

        foreach (var d in adapter.BondedDevices?.ToList() ?? new List<BluetoothDevice>())
        {
            lista.Add((d.Name ?? "(sin nombre)", d.Address ?? ""));
        }

        if (lista.Count == 0)
            UltimoMensaje = "No hay dispositivos emparejados. Empareja el módulo en Ajustes de Bluetooth.";

        return lista;
    }

    /// <summary>Conecta con el dispositivo cuya dirección MAC se indica y la recuerda.</summary>
    public async Task<bool> ConectarAAsync(string direccion)
    {
        try
        {
            // Si ya hay otro módulo conectado, cerramos antes de cambiar.
            if (_socket?.IsConnected == true)
                Disconnect();

            if (!await PedirPermisoAsync())
                return false;

            var adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter is null || !adapter.IsEnabled)
                return Fallar("El Bluetooth del tablet está apagado.");

            var dispositivo = adapter.GetRemoteDevice(direccion);
            if (dispositivo is null)
                return Fallar($"No se encontró el dispositivo con dirección {direccion}.");

            _socket = dispositivo.CreateRfcommSocketToServiceRecord(SppUuid);
            await Task.Run(() => _socket.Connect());

            _inputStream = _socket.InputStream;
            NombreConectado = dispositivo.Name ?? direccion;

            Preferences.Default.Set(ClaveDireccion, direccion);

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => EscucharDatos(_cts.Token));

            UltimoMensaje = $"Conectado a {NombreConectado}.";
            ConnectionStateChanged?.Invoke(this, true);
            return true;
        }
        catch (Java.IO.IOException ex)
        {
            return Fallar($"No se pudo abrir el canal: {ex.Message}. " +
                          "Revisa que el módulo esté encendido y que no lo use otro equipo.");
        }
        catch (Java.Lang.SecurityException ex)
        {
            return Fallar($"Android bloqueó la conexión por permisos: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Fallar($"Error inesperado: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>Reconecta con el último módulo elegido, si existe.</summary>
    public Task<bool> ConnectAsync()
    {
        var guardada = Preferences.Default.Get(ClaveDireccion, string.Empty);

        if (string.IsNullOrEmpty(guardada))
            return Task.FromResult(Fallar("Aún no has elegido un módulo. Toca Conectar y selecciona el tuyo."));

        return ConectarAAsync(guardada);
    }

    public void Disconnect()
    {
        _cts?.Cancel();

        try { _inputStream?.Close(); } catch { }
        try { _socket?.Close(); } catch { }

        _socket = null;
        _inputStream = null;
        NombreConectado = null;
        UltimoMensaje = "Desconectado.";
        ConnectionStateChanged?.Invoke(this, false);
    }

    private bool Fallar(string motivo)
    {
        UltimoMensaje = motivo;
        System.Diagnostics.Debug.WriteLine($"[BT] {motivo}");
        ConnectionStateChanged?.Invoke(this, false);
        return false;
    }

    private void EscucharDatos(CancellationToken token)
    {
        var buffer = new byte[1024];
        var linea = new StringBuilder();

        try
        {
            while (!token.IsCancellationRequested && _inputStream is not null)
            {
                int n = _inputStream.Read(buffer, 0, buffer.Length);
                if (n <= 0)
                    break;

                for (int i = 0; i < n; i++)
                {
                    char c = (char)buffer[i];

                    if (c == '\n')
                    {
                        string texto = linea.ToString().Trim();
                        linea.Clear();

                        if (texto.Length > 0)
                            DataReceived?.Invoke(this, texto);
                    }
                    else
                    {
                        linea.Append(c);
                    }
                }
            }
        }
        catch (Java.IO.IOException)
        {
            // La conexión se cayó: se notifica abajo.
        }

        if (!token.IsCancellationRequested)
        {
            UltimoMensaje = "Se perdió la conexión con el módulo.";
            ConnectionStateChanged?.Invoke(this, false);
        }
    }
}