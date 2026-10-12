using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AlertaTemprana.Services;

public interface INotificationService
{
    Task MostrarNotificacionAsync(string titulo, string mensaje);
}
