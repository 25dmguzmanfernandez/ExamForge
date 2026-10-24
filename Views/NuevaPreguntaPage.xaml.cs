namespace ExamForge.Views;

// Sirve para crear una pregunta nueva o ver una existente (llega por "pregunta")
[QueryProperty(nameof(Pregunta), "pregunta")]
public partial class NuevaPreguntaPage : ContentPage
{
    private object? pregunta; // TODO: cambiar a Pregunta cuando exista el modelo

    public NuevaPreguntaPage()
    {
        InitializeComponent();
    }

    public object? Pregunta
    {
        get => pregunta;
        set
        {
            pregunta = value;
            // TODO: rellenar EnunciadoEditor, OpcionAEntry..D, RespuestaPicker.SelectedIndex y TemaPicker
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // TODO: TemaPicker.ItemsSource = nombres de los temas
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }

    private async void GuardarClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        string enunciado = EnunciadoEditor.Text?.Trim() ?? string.Empty;
        string a = OpcionAEntry.Text?.Trim() ?? string.Empty;
        string b = OpcionBEntry.Text?.Trim() ?? string.Empty;
        string c = OpcionCEntry.Text?.Trim() ?? string.Empty;
        string d = OpcionDEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(enunciado))
        {
            MostrarError("El enunciado es obligatorio.");
            return;
        }

        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) ||
            string.IsNullOrEmpty(c) || string.IsNullOrEmpty(d))
        {
            MostrarError("Las cuatro opciones son obligatorias.");
            return;
        }

        if (RespuestaPicker.SelectedIndex == -1)
        {
            MostrarError("Selecciona la respuesta correcta.");
            return;
        }

        if (TemaPicker.SelectedIndex == -1)
        {
            MostrarError("Selecciona un tema.");
            return;
        }

        int respuestaCorrecta = RespuestaPicker.SelectedIndex; // 0-3, como en el modelo
        string tema = TemaPicker.SelectedItem?.ToString() ?? string.Empty;

        try
        {
            // TODO: crear Pregunta(enunciado, a, b, c, d, respuestaCorrecta, tema) y guardarla
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar la pregunta: {ex.Message}", "OK");
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
