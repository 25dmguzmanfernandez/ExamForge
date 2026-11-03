using System.Collections.ObjectModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.Views;

public partial class AsignaturasPage : ContentPage
{
    private readonly ObservableCollection<Asignatura> asignaturas = new();
    private readonly ObservableCollection<Tema> temas = new();
    private Asignatura? asignaturaActual;

    public AsignaturasPage()
    {
        InitializeComponent();
        ListaAsignaturas.ItemsSource = asignaturas;
        ListaTemas.ItemsSource = temas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await DataStorage.CargarAsync();
            RecargarAsignaturas();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar las asignaturas: {ex.Message}", "Aceptar");
        }
    }

    private void RecargarAsignaturas()
    {
        string? idAnterior = asignaturaActual?.Id;
        asignaturas.Clear();
        foreach (Asignatura asignatura in DataService.ObtenerAsignaturas())
            asignaturas.Add(asignatura);

        asignaturaActual = asignaturas.FirstOrDefault(a => a.Id == idAnterior);
        ListaAsignaturas.SelectedItem = asignaturaActual;
        RecargarTemas();
    }

    private void RecargarTemas()
    {
        temas.Clear();
        TemasLabel.Text = asignaturaActual is null
            ? "Temas (elige una asignatura)"
            : $"Temas de {asignaturaActual.Nombre}";

        if (asignaturaActual is null) return;
        foreach (Tema tema in asignaturaActual.Temas)
            temas.Add(tema);
    }

    private void AsignaturaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        asignaturaActual = e.CurrentSelection.FirstOrDefault() as Asignatura;
        RecargarTemas();
    }

    private async void AgregarAsignaturaClicked(object sender, EventArgs e)
    {
        string nombre = NombreEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlertAsync("Aviso", "Escribe el nombre de la asignatura.", "Aceptar");
            return;
        }

        try
        {
            await DataStorage.CargarAsync();
            Curso curso = DataService.ObtenerOCrearCursoPrincipal();
            Asignatura nueva = new() { Nombre = nombre };
            curso.Asignaturas.Add(nueva);
            try
            {
                await DataStorage.GuardarAsync();
            }
            catch
            {
                curso.Asignaturas.Remove(nueva);
                throw;
            }

            asignaturas.Add(nueva);
            ListaAsignaturas.SelectedItem = nueva;
            NombreEntry.Text = "";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar la asignatura: {ex.Message}", "Aceptar");
        }
    }

    private async void AgregarTemaClicked(object sender, EventArgs e)
    {
        if (asignaturaActual is null)
        {
            await DisplayAlertAsync("Aviso", "Selecciona primero una asignatura.", "Aceptar");
            return;
        }

        string nombre = NombreEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlertAsync("Aviso", "Escribe el nombre del tema.", "Aceptar");
            return;
        }

        Tema nuevo = new() { Nombre = nombre };
        asignaturaActual.Temas.Add(nuevo);
        try
        {
            await DataStorage.GuardarAsync();
            temas.Add(nuevo);
            NombreEntry.Text = "";
        }
        catch (Exception ex)
        {
            asignaturaActual.Temas.Remove(nuevo);
            await DisplayAlertAsync("Error", $"No se pudo guardar el tema: {ex.Message}", "Aceptar");
        }
    }
}
