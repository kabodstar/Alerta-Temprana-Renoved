using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AlertaTemprana.Services;

public enum EstadoAlerta
{
    Seguro,
    Riesgo,
    Peligro
}

public class ResultadoNivelAgua
{
    public double NivelMetros { get; init; }
    public EstadoAlerta Estado { get; init; }
}

public static class NivelAguaEvaluator
{
    // Altura total del contenedor, desde el sensor hasta la base (en cm).
    private const double AlturaTotalCm = 200.0;

    // Mismos umbrales que ya usa el Arduino, en centímetros de DISTANCIA
    // (no de nivel de agua): a menor distancia, más cerca está el agua del sensor.
    private const double UmbralRiesgoCm = 28.0;
    private const double UmbralPeligroCm = 23.0;

    public static ResultadoNivelAgua Evaluar(double distanciaCm)
    {
        // Protege contra lecturas fuera de rango (ruido del sensor, cables sueltos, etc.)
        double distanciaAcotada = Math.Clamp(distanciaCm, 0, AlturaTotalCm);

        double nivelCm = AlturaTotalCm - distanciaAcotada;
        double nivelMetros = nivelCm / 100.0;

        EstadoAlerta estado;

        if (distanciaAcotada > UmbralRiesgoCm)
            estado = EstadoAlerta.Seguro;
        else if (distanciaAcotada > UmbralPeligroCm)
            estado = EstadoAlerta.Riesgo;
        else
            estado = EstadoAlerta.Peligro;

        return new ResultadoNivelAgua
        {
            NivelMetros = nivelMetros,
            Estado = estado
        };
    }
}