using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using AlertaTemprana.Services;
using Microsoft.Maui.ApplicationModel;

namespace AlertaTemprana.Platforms.Android.Services;

public class NotificacionService : INotificationService
{
    private const string CanalId = "alerta_temprana_canal";
    private const string CanalNombre = "Alertas de nivel de agua";

    private static int _idNotificacion = 0;

    public async Task MostrarNotificacionAsync(string titulo, string mensaje)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var estadoPermiso = await Permissions.RequestAsync<NotificacionesPermission>();
            if (estadoPermiso != PermissionStatus.Granted)
                return; // El usuario no dio permiso: no podemos mostrar nada.
        }

        CrearCanalSiNoExiste();

        var contexto = global::Android.App.Application.Context;

        var notificacion = new NotificationCompat.Builder(contexto, CanalId)
            .SetContentTitle(titulo)
            .SetContentText(mensaje)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogAlert)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetAutoCancel(true)
            .Build();

        NotificationManagerCompat.From(contexto).Notify(_idNotificacion++, notificacion);
    }

    private void CrearCanalSiNoExiste()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return; // Los canales de notificación no existen antes de Android 8.

        var canal = new NotificationChannel(CanalId, CanalNombre, NotificationImportance.High)
        {
            Description = "Avisos cuando el nivel de agua llega a Riesgo o Peligro."
        };

        var administrador = (NotificationManager)global::Android.App.Application.Context
            .GetSystemService(Context.NotificationService)!;

        administrador.CreateNotificationChannel(canal);
    }
}