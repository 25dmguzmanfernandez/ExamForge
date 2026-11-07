/*
 * NavbarView.xaml.cs: lógica visual de la barra superior.
 * MostrarAtras / MostrarSesion: muestran u ocultan los botones (se ponen desde el XAML de cada página).
 * Eventos AtrasClicked y PerfilClicked: los atiende la página que contiene la barra.
 * OnSalir: confirma, marca la sesión como cerrada y vuelve al portal.
 */
using ExamForge.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExamForge.Views.Componentes;

public partial class NavbarView : ContentView
{
    public event EventHandler? AtrasClicked;
    public event EventHandler? PerfilClicked;

    public NavbarView()
    {
        InitializeComponent();
    }

    public bool MostrarAtras
    {
        get => AtrasButton.IsVisible;
        set => AtrasButton.IsVisible = value;
    }

    public bool MostrarSesion
    {
        get => SalirButton.IsVisible;
        set
        {
            PerfilButton.IsVisible = value;
            SalirButton.IsVisible = value;
        }
    }

    private void OnAtras(object sender, EventArgs e) => AtrasClicked?.Invoke(this, e);

    private void OnPerfil(object sender, EventArgs e) => PerfilClicked?.Invoke(this, e);

    private async void OnSalir(object sender, EventArgs e)
    {
        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Cerrar sesión",
            "¿Seguro que quieres cerrar sesión?",
            "Cerrar sesión",
            "Cancelar");

        if (!confirmar)
            return;

        var dataService = Handler?.MauiContext?.Services.GetService<DataService>();

        if (dataService is not null)
            dataService.UsuarioActivo = null;

        await Shell.Current.GoToAsync("//portal");
    }
}
