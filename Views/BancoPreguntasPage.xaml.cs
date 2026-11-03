namespace ExamForge.Views;

public partial class BancoPreguntasPage : ContentPage
{
    public BancoPreguntasPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Se recarga cada vez que se vuelve de NuevaPregunta
        // TODO: ListaPreguntas.ItemsSource = gestor.Preguntas;
    }

    private async void AnadirPreguntaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage));
    }

    private async void PreguntaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        object? pregunta = e.CurrentSelection.FirstOrDefault();

        if (pregunta == null)
            return;

        // Se limpia la selección para poder volver a pulsar el mismo elemento
        ListaPreguntas.SelectedItem = null;

        await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage), new Dictionary<string, object>
        {
            { "pregunta", pregunta }
        });
    }
}
