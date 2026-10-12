
namespace AlertaTemprana.Services;

public static class EstadoAlertaPresentacion
{
    public static Color ObtenerColor(EstadoAlerta estado)
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
}