
using AlertaTemprana.Models;
using AlertaTemprana.Services;

namespace AlertaTemprana;

public partial class Historialpage : ContentPage
{
    private readonly HistorialServices _historialService = new();

    // Guarda TODOS los registros originales.
    // Los filtros trabajan sobre esta lista.
    private List<HistorialEntry> _registros = new();

    // Filtros actualmente seleccionados.
    private RangoFiltro _rangoActual = RangoFiltro.Ultimas24Horas;
    private EstadoFiltro _estadoActual = EstadoFiltro.Todos;

    // =========================
    // FILTROS
    // =========================

    private enum RangoFiltro
    {
        Ultimas24Horas,
        Ultimos7Dias,
        Ultimos30Dias,
        Todos
    }

    private enum EstadoFiltro
    {
        Todos,
        Seguro,
        Riesgo,
        Peligro
    }

    // =========================
    // CONSTRUCTOR
    // =========================

    public Historialpage()
    {
        InitializeComponent();
    }

    // =========================
    // CUANDO SE ABRE HISTORIAL
    // =========================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CargarHistorialAsync();
    }

    // =========================
    // CARGAR HISTORIAL
    // =========================

    private async Task CargarHistorialAsync()
    {
        _registros = await _historialService.ObtenerHistorialAsync();

        AplicarFiltros();
    }

    // =========================
    // APLICAR FILTROS
    // =========================

    private void AplicarFiltros()
    {
        IEnumerable<HistorialEntry> resultados = _registros;

        // ---------------------------------
        // FILTRO POR FECHA
        // ---------------------------------

        DateTime ahora = DateTime.Now;

        switch (_rangoActual)
        {
            case RangoFiltro.Ultimas24Horas:

                resultados = resultados.Where(r =>
                    r.FechaHora >= ahora.AddHours(-24));

                break;

            case RangoFiltro.Ultimos7Dias:

                resultados = resultados.Where(r =>
                    r.FechaHora >= ahora.AddDays(-7));

                break;

            case RangoFiltro.Ultimos30Dias:

                resultados = resultados.Where(r =>
                    r.FechaHora >= ahora.AddDays(-30));

                break;

            case RangoFiltro.Todos:

                // No se aplica ningún filtro de fecha.
                break;
        }

        // ---------------------------------
        // FILTRO POR ESTADO
        // ---------------------------------

        switch (_estadoActual)
        {
            case EstadoFiltro.Seguro:

                resultados = resultados.Where(r =>
                    r.Estado == EstadoAlerta.Seguro);

                break;

            case EstadoFiltro.Riesgo:

                resultados = resultados.Where(r =>
                    r.Estado == EstadoAlerta.Riesgo);

                break;

            case EstadoFiltro.Peligro:

                resultados = resultados.Where(r =>
                    r.Estado == EstadoAlerta.Peligro);

                break;

            case EstadoFiltro.Todos:

                // No se aplica filtro de estado.
                break;
        }

        // ---------------------------------
        // ORDENAR Y MOSTRAR
        // ---------------------------------

        List<HistorialItemView> listaVisual = resultados
            .OrderByDescending(r => r.FechaHora)
            .Select(ConvertirAItem)
            .ToList();

        CvHistorial.ItemsSource = listaVisual;
    }

    // =========================
    // CONVERTIR REGISTRO
    // =========================

    private static HistorialItemView ConvertirAItem(
        HistorialEntry registro)
    {
        return new HistorialItemView
        {
            NivelTexto = registro.NivelMetros.ToString("F2"),

            EstadoTexto = ObtenerTextoHistorial(
                registro.Estado),

            EstadoColor = EstadoAlertaPresentacion
                .ObtenerColor(registro.Estado),

            EstadoBrush = new SolidColorBrush(
                EstadoAlertaPresentacion
                    .ObtenerColor(registro.Estado)),

            FechaTexto = registro.FechaHora
                .ToString("dd MMM yyyy, h:mm tt"),

            AlturaBarraMini = Math.Clamp(
                (registro.NivelMetros / 2.0) * 110,
                0,
                110)
        };
    }

    // =========================
    // TEXTO DEL ESTADO
    // =========================

    private static string ObtenerTextoHistorial(
        EstadoAlerta estado)
    {
        return estado switch
        {
            EstadoAlerta.Seguro => "Nivel normal",
            EstadoAlerta.Riesgo => "Nivel de riesgo",
            EstadoAlerta.Peligro => "Nivel de peligro",

            _ => string.Empty
        };
    }

    // =========================
    // BOTÓN CALENDARIO
    // =========================

    private async void OnCalendarioClicked(
        object sender,
        EventArgs e)
    {
        await MostrarFiltroRangoAsync();
    }

    // =========================
    // BOTÓN "FILTRAR"
    // =========================

    private async void OnFiltrarTapped(
        object sender,
        EventArgs e)
    {
        string? opcion = await DisplayActionSheet(
            "Filtrar historial",
            "Cancelar",
            null,
            "Rango de tiempo",
            "Tipo de evento",
            "Restablecer filtros");

        switch (opcion)
        {
            case "Rango de tiempo":

                await MostrarFiltroRangoAsync();

                break;

            case "Tipo de evento":

                await MostrarFiltroEventosAsync();

                break;

            case "Restablecer filtros":

                _rangoActual = RangoFiltro.Todos;
                _estadoActual = EstadoFiltro.Todos;

                ActualizarTextosFiltros();

                AplicarFiltros();

                break;
        }
    }

    // =========================
    // FILTRO DE RANGO
    // =========================

    private async void OnFiltroRangoTapped(
        object sender,
        EventArgs e)
    {
        await MostrarFiltroRangoAsync();
    }

    private async Task MostrarFiltroRangoAsync()
    {
        string? opcion = await DisplayActionSheet(
            "Filtrar por tiempo",
            "Cancelar",
            null,
            "Últimas 24 horas",
            "Últimos 7 días",
            "Últimos 30 días",
            "Todos los registros");

        switch (opcion)
        {
            case "Últimas 24 horas":

                _rangoActual =
                    RangoFiltro.Ultimas24Horas;

                break;

            case "Últimos 7 días":

                _rangoActual =
                    RangoFiltro.Ultimos7Dias;

                break;

            case "Últimos 30 días":

                _rangoActual =
                    RangoFiltro.Ultimos30Dias;

                break;

            case "Todos los registros":

                _rangoActual =
                    RangoFiltro.Todos;

                break;

            default:

                return;
        }

        ActualizarTextosFiltros();

        AplicarFiltros();
    }

    // =========================
    // FILTRO DE EVENTOS
    // =========================

    private async void OnFiltroEventosTapped(
        object sender,
        EventArgs e)
    {
        await MostrarFiltroEventosAsync();
    }

    private async Task MostrarFiltroEventosAsync()
    {
        string? opcion = await DisplayActionSheet(
            "Filtrar por evento",
            "Cancelar",
            null,
            "Todos los eventos",
            "Nivel normal",
            "Nivel de riesgo",
            "Nivel de peligro");

        switch (opcion)
        {
            case "Todos los eventos":

                _estadoActual =
                    EstadoFiltro.Todos;

                break;

            case "Nivel normal":

                _estadoActual =
                    EstadoFiltro.Seguro;

                break;

            case "Nivel de riesgo":

                _estadoActual =
                    EstadoFiltro.Riesgo;

                break;

            case "Nivel de peligro":

                _estadoActual =
                    EstadoFiltro.Peligro;

                break;

            default:

                return;
        }

        ActualizarTextosFiltros();

        AplicarFiltros();
    }

    // =========================
    // ACTUALIZAR LOS TEXTOS
    // =========================

    private void ActualizarTextosFiltros()
    {
        // Filtro de fecha

        LblFiltroRango.Text = _rangoActual switch
        {
            RangoFiltro.Ultimas24Horas =>
                "Últimas 24 horas",

            RangoFiltro.Ultimos7Dias =>
                "Últimos 7 días",

            RangoFiltro.Ultimos30Dias =>
                "Últimos 30 días",

            RangoFiltro.Todos =>
                "Todos los registros",

            _ => "Todos los registros"
        };

        // Filtro de evento

        LblFiltroEventos.Text = _estadoActual switch
        {
            EstadoFiltro.Todos =>
                "Todos los eventos",

            EstadoFiltro.Seguro =>
                "Nivel normal",

            EstadoFiltro.Riesgo =>
                "Nivel de riesgo",

            EstadoFiltro.Peligro =>
                "Nivel de peligro",

            _ => "Todos los eventos"
        };
    }

    // =========================
    // BORRAR HISTORIAL
    // =========================

    private async void OnBorrarHistorialClicked(
        object sender,
        EventArgs e)
    {
        bool confirmar = await DisplayAlert(
            "Borrar historial",
            "¿Seguro que quieres borrar todos los registros del historial? Esta acción no se puede deshacer.",
            "Borrar",
            "Cancelar");

        if (!confirmar)
            return;

        await _historialService
            .LimpiarHistorialAsync();

        _registros.Clear();

        AplicarFiltros();
    }
}
