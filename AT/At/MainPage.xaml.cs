using AlertaTemprana.Models;
using AlertaTemprana.Services;

#if ANDROID
using AlertaTemprana.Platforms.Android.Services;
#endif


namespace AlertaTemprana;

public partial class MainPage : ContentPage
{
    // Cambia esto a "false" el día que tengas la maqueta real conectada
    // y quieras usar el HC-06 de verdad en vez de datos simulados.
    private const bool UsarSimulador = false;

    private readonly IWaterLevelSource _waterLevelSource;
    private readonly HistorialServices _historialService = new();
    private readonly INotificationService _notificationService;
    private EstadoAlerta? _ultimoEstadoRegistrado;

    public MainPage()
    {
        InitializeComponent();

#if ANDROID
        _waterLevelSource = UsarSimulador
            ? new SimulatedWaterLevelSource()
            : new BluetoothService();

        _notificationService = new NotificacionService();
#else
        _waterLevelSource = new SimulatedWaterLevelSource();
        _notificationService = new NotificationServiceNoOp();
#endif

        // El botón manual solo tiene sentido cuando usamos el Bluetooth real.
        BtnConectar.IsVisible = !UsarSimulador;
        LblEstadoConexion.IsVisible = !UsarSimulador;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _waterLevelSource.DataReceived += OnDatoRecibido;
        _waterLevelSource.ConnectionStateChanged += OnEstadoConexionCambiado;

        if (UsarSimulador)
        {
            // El simulador es barato de conectar/desconectar: lo hacemos automático.
            await _waterLevelSource.ConnectAsync();
        }
        // Si es Bluetooth real, NO conectamos aquí — el usuario lo hace con el botón.
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _waterLevelSource.DataReceived -= OnDatoRecibido;
        _waterLevelSource.ConnectionStateChanged -= OnEstadoConexionCambiado;

        if (UsarSimulador)
        {
            _waterLevelSource.Disconnect();
        }
        // Si es Bluetooth real, NO desconectamos aquí — la conexión sigue viva
        // aunque el usuario navegue a Historial y regrese a Inicio.
    }

    private void OnDatoRecibido(object? sender, string distanciaCmTexto)
    {
        if (!double.TryParse(distanciaCmTexto, out double distanciaCm))
            return; // Dato no numérico (por ejemplo "PELIGRO"): lo ignoramos.

        // NUEVO: lectura inválida (0 o negativa). No debe generar alertas falsas.
        if (distanciaCm <= 0)
            return;

        ResultadoNivelAgua resultado = NivelAguaEvaluator.Evaluar(distanciaCm);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblNivelAguaValor.Text = resultado.NivelMetros.ToString("F2");

            double alturaBarra = (resultado.NivelMetros / 2.0) * 200;
            BoxViewNivelAgua.HeightRequest = Math.Clamp(alturaBarra, 0, 200);

            LblEstadoTitulo.Text = ObtenerTextoEstado(resultado.Estado);
            LblEstadoTitulo.TextColor = ObtenerColorEstado(resultado.Estado);

            LblEstadoDescripcion.Text = ObtenerDescripcionEstado(resultado.Estado);
        });

        RegistrarSiCambioDeEstado(resultado);
    }

    private void RegistrarSiCambioDeEstado(ResultadoNivelAgua resultado)
    {
        if (_ultimoEstadoRegistrado == resultado.Estado)
            return;

        _ultimoEstadoRegistrado = resultado.Estado;

        if (resultado.Estado == EstadoAlerta.Seguro)
            return;

        var evento = new HistorialEntry
        {
            FechaHora = DateTime.Now,
            NivelMetros = resultado.NivelMetros,
            Estado = resultado.Estado
        };

        _ = _historialService.GuardarEventoAsync(evento);

        string tituloNotificacion = $"Alerta Temprana: {ObtenerTextoEstado(resultado.Estado)}";
        string mensajeNotificacion = ObtenerDescripcionEstado(resultado.Estado);
        _ = _notificationService.MostrarNotificacionAsync(tituloNotificacion, mensajeNotificacion);

        System.Diagnostics.Debug.WriteLine(
            $"[Historial] Registrado: {evento.FechaHora:HH:mm:ss} - {evento.Estado} - {evento.NivelMetros:F2} m");
    }

    private static string ObtenerTextoEstado(EstadoAlerta estado) => estado switch
    {
        EstadoAlerta.Seguro => "SEGURO",
        EstadoAlerta.Riesgo => "RIESGO",
        EstadoAlerta.Peligro => "PELIGRO",
        _ => string.Empty
    };

    private static Color ObtenerColorEstado(EstadoAlerta estado)
    {
        string nombreRecurso = estado switch
        {
            EstadoAlerta.Seguro => "AlertGreen",
            EstadoAlerta.Riesgo => "AlertYellow",
            EstadoAlerta.Peligro => "AlertRed",
            _ => "TextDarkNavy"
        };

        return (Color)Application.Current!.Resources[nombreRecurso];
    }

    private static string ObtenerDescripcionEstado(EstadoAlerta estado) => estado switch
    {
        EstadoAlerta.Seguro => "El nivel del agua se encuentra en rangos normales.",
        EstadoAlerta.Riesgo => "El nivel del agua está subiendo. Mantente atento.",
        EstadoAlerta.Peligro => "Nivel de agua crítico. Toma precauciones inmediatas.",
        _ => string.Empty
    };

    private void OnEstadoConexionCambiado(object? sender, bool conectado)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblEstadoConexion.Text = conectado ? "Bluetooth: conectado" : "Bluetooth: desconectado";
            LblEstadoConexion.TextColor = conectado
                ? (Color)Application.Current!.Resources["AlertGreen"]
                : (Color)Application.Current!.Resources["AlertRed"];
        });

        System.Diagnostics.Debug.WriteLine(conectado ? "[Datos] Conectado" : "[Datos] Desconectado");
    }

    private async void OnConectarClicked(object sender, EventArgs e)
    {
#if ANDROID
        if (_waterLevelSource is not BluetoothService bt)
            return;

        if (!await bt.PedirPermisoAsync())
        {
            await DisplayAlert("No conecta", bt.UltimoMensaje, "OK");
            return;
        }

        var emparejados = bt.ObtenerEmparejados();
        if (emparejados.Count == 0)
        {
            await DisplayAlert("No conecta", bt.UltimoMensaje, "OK");
            return;
        }

        var opciones = emparejados
            .Select(d => $"{d.Nombre} ({d.Direccion})")
            .ToArray();

        string? elegido = await DisplayActionSheet(
            "Elige tu módulo de nivel", "Cancelar", null, opciones);

        if (string.IsNullOrEmpty(elegido) || elegido == "Cancelar")
            return;

        int indice = Array.IndexOf(opciones, elegido);
        if (indice < 0)
            return;

        BtnConectar.IsEnabled = false;
        BtnConectar.Text = "Conectando...";

        bool conectado = await bt.ConectarAAsync(emparejados[indice].Direccion);

        BtnConectar.IsEnabled = true;
        BtnConectar.Text = conectado
            ? $"Cambiar módulo ({bt.NombreConectado})"
            : "Conectar módulo";

        if (!conectado)
            await DisplayAlert("No conecta", bt.UltimoMensaje, "OK");
#endif
    }

    private void ActualizarEstado(double nivel)
    {

        string estado;

        if (nivel < 30)
        {
            estado = "seguro";
        }
        else if (nivel < 70)
        {
            estado = "precaucion";
        }
        else
        {
            estado = "peligro";
        }

        ActualizarImagenEstado(estado);
    }

    private void ActualizarImagenEstado(string estado)
    {
        switch (estado.ToLower())
        {
            case "seguro":
                ImagenEstado.Source = "";
                break;

            case "precaucion":
                ImagenEstado.Source = "";
                break;

            case "peligro":
                ImagenEstado.Source = "";
                break;

            default:
                ImagenEstado.Source = "-";
                break;
        }
    }
}