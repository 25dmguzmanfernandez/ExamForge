using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.Views;

public sealed class TemaPickerItem
{
    public Asignatura Asignatura { get; }
    public Tema Tema { get; }
    public string Etiqueta => $"{Asignatura.Nombre} · {Tema.Nombre}";

    public TemaPickerItem(Asignatura asignatura, Tema tema)
    {
        Asignatura = asignatura;
        Tema = tema;
    }
}

// Se usa tanto para crear como para editar una pregunta existente.
[QueryProperty(nameof(Pregunta), "pregunta")]
public partial class NuevaPreguntaPage : ContentPage
{
    private Pregunta? preguntaActual;
    private bool formularioPreparado;
    private List<TemaPickerItem> temasDisponibles = new();

    public NuevaPreguntaPage()
    {
        InitializeComponent();
    }

    public Pregunta? Pregunta
    {
        get => preguntaActual;
        set => preguntaActual = value;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (formularioPreparado) return;

        try
        {
            await DataStorage.CargarAsync();
            temasDisponibles = DataService.ObtenerTemas()
                .Select(par => new TemaPickerItem(par.Asignatura, par.Tema))
                .ToList();
            TemaPicker.ItemsSource = temasDisponibles;

            if (preguntaActual is not null)
            {
                Title = "Editar pregunta";
                EnunciadoEditor.Text = preguntaActual.Enunciado;
                OpcionAEntry.Text = preguntaActual.Opciones.ElementAtOrDefault(0) ?? "";
                OpcionBEntry.Text = preguntaActual.Opciones.ElementAtOrDefault(1) ?? "";
                OpcionCEntry.Text = preguntaActual.Opciones.ElementAtOrDefault(2) ?? "";
                OpcionDEntry.Text = preguntaActual.Opciones.ElementAtOrDefault(3) ?? "";
                RespuestaPicker.SelectedIndex = preguntaActual.RespuestaCorrecta;

                Tema? temaOriginal = DataService.BuscarTemaDePregunta(preguntaActual.Id);
                TemaPicker.SelectedItem = temasDisponibles
                    .FirstOrDefault(t => t.Tema.Id == temaOriginal?.Id);
            }

            formularioPreparado = true;
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudieron cargar los temas: {ex.Message}");
            GuardarButton.IsEnabled = false;
        }
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }

    private async void GuardarClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        string enunciado = EnunciadoEditor.Text?.Trim() ?? "";
        string a = OpcionAEntry.Text?.Trim() ?? "";
        string b = OpcionBEntry.Text?.Trim() ?? "";
        string c = OpcionCEntry.Text?.Trim() ?? "";
        string d = OpcionDEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(enunciado))
        {
            MostrarError("El enunciado es obligatorio.");
            return;
        }
        if (new[] { a, b, c, d }.Any(string.IsNullOrWhiteSpace))
        {
            MostrarError("Las cuatro opciones son obligatorias.");
            return;
        }
        if (RespuestaPicker.SelectedIndex < 0)
        {
            MostrarError("Selecciona la respuesta correcta.");
            return;
        }
        if (TemaPicker.SelectedItem is not TemaPickerItem temaElegido)
        {
            MostrarError("Selecciona un tema. Si no hay temas, créalos en Asignaturas.");
            return;
        }

        var opciones = new List<string> { a, b, c, d };
        var nuevaVersion = new Pregunta
        {
            Enunciado = enunciado,
            Opciones = opciones,
            RespuestaCorrecta = RespuestaPicker.SelectedIndex,
            FeedbackExplicacion = preguntaActual?.FeedbackExplicacion ?? ""
        };

        if (!nuevaVersion.EsValida())
        {
            MostrarError("Revisa los datos de la pregunta.");
            return;
        }

        GuardarButton.IsEnabled = false;
        bool guardada = false;

        if (preguntaActual is null)
        {
            temaElegido.Tema.BancoDePreguntas.Add(nuevaVersion);
            try
            {
                await DataStorage.GuardarAsync();
                guardada = true;
            }
            catch (Exception ex)
            {
                temaElegido.Tema.BancoDePreguntas.Remove(nuevaVersion);
                MostrarError($"No se pudo guardar la pregunta: {ex.Message}");
            }
        }
        else
        {
            Tema? temaOriginal = DataService.BuscarTemaDePregunta(preguntaActual.Id);
            if (temaOriginal is null)
            {
                MostrarError("No se ha encontrado el tema original de la pregunta.");
                GuardarButton.IsEnabled = true;
                return;
            }

            // Conservamos el Id para no romper los vínculos con cuestionarios.
            string textoAnterior = preguntaActual.Enunciado;
            List<string> opcionesAnteriores = preguntaActual.Opciones.ToList();
            int correctaAnterior = preguntaActual.RespuestaCorrecta;
            bool cambiarTema = temaOriginal.Id != temaElegido.Tema.Id;

            preguntaActual.Enunciado = enunciado;
            preguntaActual.Opciones = opciones;
            preguntaActual.RespuestaCorrecta = RespuestaPicker.SelectedIndex;
            if (cambiarTema)
            {
                temaOriginal.BancoDePreguntas.Remove(preguntaActual);
                temaElegido.Tema.BancoDePreguntas.Add(preguntaActual);
            }

            try
            {
                await DataStorage.GuardarAsync();
                guardada = true;
            }
            catch (Exception ex)
            {
                preguntaActual.Enunciado = textoAnterior;
                preguntaActual.Opciones = opcionesAnteriores;
                preguntaActual.RespuestaCorrecta = correctaAnterior;
                if (cambiarTema)
                {
                    temaElegido.Tema.BancoDePreguntas.Remove(preguntaActual);
                    temaOriginal.BancoDePreguntas.Add(preguntaActual);
                }
                MostrarError($"No se pudieron guardar los cambios: {ex.Message}");
            }
        }

        GuardarButton.IsEnabled = true;
        if (guardada)
            await Shell.Current.GoToAsync("..");
    }

    private async void CancelarClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
