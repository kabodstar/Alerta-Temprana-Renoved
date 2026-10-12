using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlertaTemprana.Services;

namespace AlertaTemprana.Models;

public class HistorialEntry
{
    public DateTime FechaHora { get; set; }
    public double NivelMetros { get; set; }
    public EstadoAlerta Estado { get; set; }
}