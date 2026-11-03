using System.Collections.ObjectModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.Views;

// El modelo Pregunta no tiene un campo Tema: su tema es el contenedor.
// Esta clase aporta el nombre del tema a la tarjeta sin modificar el modelo.
public sealed class PreguntaBancoItem
{
    public Pregunta Pregunta { get; }
    public Tema TemaOriginal { get; }
    public string Enunciado => Pregunta.Enunciado;
    public string Tema { get; }

    public PreguntaBancoItem(Pregunta pregunta, Tema tema, Asignatura asignatura)
    {
        Pregunta = pregunta;
        TemaOriginal = tema;
        Tema = $"{asignatura.Nombre} · {tema.Nombre}";
    }
}

public partial class BancoPreguntasPage : ContentPage
{
    private readonly ObservableCollection<PreguntaBancoItem> preguntas = new();

    public BancoPreguntasPage()
    {
        InitializeComponent();
        ListaPreguntas.ItemsSource = preguntas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await DataStorage.CargarAsync();
            RecargarPreguntas();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar las preguntas: {ex.Message}", "Aceptar");
        }
    }

    private void RecargarPreguntas()
    {
        preguntas.Clear();
        foreach (var (asignatura, tema) in DataService.ObtenerTemas())
            foreach (Pregunta pregunta in tema.BancoDePreguntas)
                preguntas.Add(new PreguntaBancoItem(pregunta, tema, asignatura));
    }

    private async void AnadirPreguntaClicked(object sender, EventArgs e)
    {
        try
        {
            await DataStorage.CargarAsync();
            if (!DataService.ObtenerTemas().Any())
            {
                await DisplayAlertAsync("Primero crea un tema", "En Asignaturas, crea una asignatura y un tema para guardar preguntas.", "Aceptar");
                return;
            }
            await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    private async void PreguntaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PreguntaBancoItem item) return;
        ListaPreguntas.SelectedItem = null;

        await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage), new Dictionary<string, object>
        {
            { "pregunta", item.Pregunta }
        });
    }

    private async void OnEliminarPreguntaClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not PreguntaBancoItem item)
            return;

        bool confirmar = await DisplayAlertAsync(
            "Eliminar pregunta", "¿Seguro que quieres eliminar esta pregunta?", "Eliminar", "Cancelar");
        if (!confirmar) return;

        // Borramos del tema real, no solo de la lista que muestra la pantalla.
        if (!item.TemaOriginal.BancoDePreguntas.Remove(item.Pregunta)) return;

        try
        {
            await DataStorage.GuardarAsync();
            preguntas.Remove(item);
        }
        catch (Exception ex)
        {
            item.TemaOriginal.BancoDePreguntas.Add(item.Pregunta);
            await DisplayAlertAsync("Error", $"No se pudo eliminar la pregunta: {ex.Message}", "Aceptar");
        }
    }
}
