using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AlertaTemprana.Services;

public interface IWaterLevelSource
{
    // Se dispara cada vez que llega un dato nuevo (real o simulado).
    // El texto siempre es un número de distancia en centímetros, como texto.
    event EventHandler<string>? DataReceived;

    // Se dispara cuando cambia el estado de conexión (true = conectado).
    event EventHandler<bool>? ConnectionStateChanged;

    Task<bool> ConnectAsync();

    void Disconnect();
}