/*
 * ObjetoVista.cs: clases base de las "vistas" (envoltorios de pantalla), igual que MisionVista en clase.
 * ObjetoVista: avisa a la interfaz cuando cambia una propiedad (INotifyPropertyChanged).
 * TarjetaVista: estado de una card: Desplegado (lista abierta), EnSeleccion (casillas visibles) y Seleccionado.
 * Solo guardan estado visual; los datos reales siguen en los modelos.
 */
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExamForge.Views.Vistas;

public abstract class ObjetoVista : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Notificar([CallerMemberName] string? propiedad = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));

    protected bool Asignar<T>(ref T campo, T valor, [CallerMemberName] string? propiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor)) return false;
        campo = valor;
        Notificar(propiedad);
        return true;
    }
}

public abstract class TarjetaVista : ObjetoVista
{
    private bool desplegado, seleccionado, enSeleccion;

    public bool Desplegado
    {
        get => desplegado;
        set { if (Asignar(ref desplegado, value)) Notificar(nameof(TextoDesplegable)); }
    }

    public bool Seleccionado { get => seleccionado; set => Asignar(ref seleccionado, value); }
    public bool EnSeleccion
    {
        get => enSeleccion;
        set
        {
            if (Asignar(ref enSeleccion, value))
                Notificar(nameof(OpacidadCasilla));
        }
    }

    public double OpacidadCasilla => EnSeleccion ? 1 : 0;
    public string TextoDesplegable => Desplegado ? "▲" : "▼";
}
