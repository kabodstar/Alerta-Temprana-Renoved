  namespace AlertaTemprana.Models;

public class HistorialItemView
{
    public string NivelTexto { get; set; } = string.Empty;
    public string EstadoTexto { get; set; } = string.Empty;
    public Color EstadoColor { get; set; } = Colors.Black;
    public Brush EstadoBrush { get; set; } = new SolidColorBrush(Colors.Black);
    public string FechaTexto { get; set; } = string.Empty;
    public double AlturaBarraMini { get; set; }
}