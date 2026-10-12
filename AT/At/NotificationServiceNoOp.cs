using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AlertaTemprana.Services;

// "NoOp" = No Operation. Cumple el contrato pero no hace nada real,
// para que la app compile y corra en plataformas donde no implementamos
// notificaciones nativas todavía (iOS, MacCatalyst, Windows).
public class NotificationServiceNoOp : INotificationService
{
    public Task MostrarNotificacionAsync(string titulo, string mensaje)
    {
        System.Diagnostics.Debug.WriteLine($"[Notificación simulada] {titulo}: {mensaje}");
        return Task.CompletedTask;
    }
}