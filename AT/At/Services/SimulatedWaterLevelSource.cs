using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AlertaTemprana.Services;

public class SimulatedWaterLevelSource : IWaterLevelSource
{
    private readonly Random _random = new();
    private IDispatcherTimer? _timer;

    // Punto de partida: distancia "lejos" del agua (equivale a estado SEGURO).
    private double _distanciaActualCm = 60;

    // -1 = el agua está subiendo (la distancia disminuye)
    //  1 = el agua está bajando (la distancia aumenta)
    private int _direccion = -1;

    public event EventHandler<string>? DataReceived;
    public event EventHandler<bool>? ConnectionStateChanged;

    public Task<bool> ConnectAsync()
    {
        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(2);
        _timer.Tick += (sender, e) => GenerarLecturaSimulada();
        _timer.Start();

        ConnectionStateChanged?.Invoke(this, true);
        return Task.FromResult(true);
    }

    private void GenerarLecturaSimulada()
    {
        // Simula que el nivel de agua sube y baja lentamente, con algo
        // de variación aleatoria, como se comportaría un sensor real.
        _distanciaActualCm += _direccion * _random.Next(1, 4);

        if (_distanciaActualCm <= 2)
        {
            _distanciaActualCm = 2;
            _direccion = 1; // rebota: ahora "baja" el aguaB
        }
        else if (_distanciaActualCm >= 90)
        {
            _distanciaActualCm = 90;
            _direccion = -1; // rebota: ahora "sube" el agua otra vez
        }

        DataReceived?.Invoke(this, _distanciaActualCm.ToString("F0"));
    }

    public void Disconnect()
    {
        _timer?.Stop();
        ConnectionStateChanged?.Invoke(this, false);
    }
}