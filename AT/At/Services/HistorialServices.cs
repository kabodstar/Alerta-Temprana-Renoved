using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using AlertaTemprana.Models;

namespace AlertaTemprana.Services;

public class HistorialServices
{
    private const string NombreArchivo = "historial_niveles.json";

    private string RutaArchivo =>
        Path.Combine(FileSystem.AppDataDirectory, NombreArchivo);

    public async Task GuardarEventoAsync(HistorialEntry nuevoEvento)
    {
        List<HistorialEntry> historialActual = await ObtenerHistorialAsync();

        historialActual.Add(nuevoEvento);

        string json = JsonSerializer.Serialize(historialActual);

        await File.WriteAllTextAsync(RutaArchivo, json);
    }

    public async Task<List<HistorialEntry>> ObtenerHistorialAsync()
    {
        if (!File.Exists(RutaArchivo))
            return new List<HistorialEntry>();

        try
        {
            string json = await File.ReadAllTextAsync(RutaArchivo);
            return JsonSerializer.Deserialize<List<HistorialEntry>>(json)
                   ?? new List<HistorialEntry>();
        }
        catch (JsonException)
        {
            // El archivo quedó corrupto o vacío: empezamos de cero en vez de romper la app.
            return new List<HistorialEntry>();
        }
    }
    public Task LimpiarHistorialAsync()
    {
        if (File.Exists(RutaArchivo))
            File.Delete(RutaArchivo);

        return Task.CompletedTask;
    }
}
