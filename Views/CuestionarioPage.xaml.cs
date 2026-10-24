namespace ExamForge.Views;

public partial class CuestionarioPage : ContentPage
{
    private int indiceActual = 0;
    private int totalPreguntas = 0;
    private int aciertos = 0;
    private int opcionSeleccionada = -1;
    private int respuestaCorrectaActual = -1;

    private Button[] botonesOpcion = Array.Empty<Button>();

    public CuestionarioPage()
    {
        InitializeComponent();
        botonesOpcion = new[] { OpcionAButton, OpcionBButton, OpcionCButton, OpcionDButton };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // TODO: obtener las preguntas del cuestionario (gestor) y llamar a:
        //   IniciarCuestionario(lista.Count);
        //   MostrarPregunta(p.Enunciado, p.OpcionA, p.OpcionB, p.OpcionC, p.OpcionD, p.RespuestaCorrecta);
    }

    // --- Métodos que usará la lógica cuando exista el modelo ---

    public void IniciarCuestionario(int total)
    {
        totalPreguntas = total;
        indiceActual = 0;
        aciertos = 0;
        PreguntaLayout.IsVisible = true;
        ResultadoLayout.IsVisible = false;
    }

    public void MostrarPregunta(string enunciado, string a, string b, string c, string d, int correcta)
    {
        opcionSeleccionada = -1;
        respuestaCorrectaActual = correcta;

        ProgresoLabel.Text = $"Pregunta {indiceActual + 1} de {totalPreguntas}";
        EnunciadoLabel.Text = enunciado;

        OpcionAButton.Text = a;
        OpcionBButton.Text = b;
        OpcionCButton.Text = c;
        OpcionDButton.Text = d;

        MarcarSeleccion(-1);

        AvanzarButton.Text = indiceActual == totalPreguntas - 1 ? "Finalizar" : "Siguiente";
    }

    // --- Eventos ---

    private void OpcionClicked(object sender, EventArgs e)
    {
        if (sender is not Button boton || !int.TryParse(boton.CommandParameter?.ToString(), out int indice))
            return;

        opcionSeleccionada = indice;
        MarcarSeleccion(indice);
    }

    private void MarcarSeleccion(int indice)
    {
        for (int i = 0; i < botonesOpcion.Length; i++)
        {
            bool seleccionado = i == indice;
            botonesOpcion[i].BackgroundColor = seleccionado
                ? (Color)Application.Current!.Resources["Acento"]
                : (Color)Application.Current!.Resources["AzulOscuro"];
            botonesOpcion[i].TextColor = seleccionado
                ? (Color)Application.Current!.Resources["AzulOscuro"]
                : Colors.White;
        }
    }

    private async void AvanzarClicked(object sender, EventArgs e)
    {
        if (opcionSeleccionada == -1)
        {
            await DisplayAlertAsync("Aviso", "Selecciona una respuesta", "OK");
            return;
        }

        if (opcionSeleccionada == respuestaCorrectaActual)
            aciertos++;

        indiceActual++;

        if (indiceActual >= totalPreguntas)
        {
            MostrarResultado();
            return;
        }

        // TODO: MostrarPregunta(...) con la pregunta número indiceActual
    }

    private void MostrarResultado()
    {
        double porcentaje = totalPreguntas == 0 ? 0 : (double)aciertos / totalPreguntas * 100;

        PorcentajeLabel.Text = $"{porcentaje:0}%";
        ResumenLabel.Text = $"Aciertos: {aciertos} de {totalPreguntas}";

        PreguntaLayout.IsVisible = false;
        ResultadoLayout.IsVisible = true;
    }

    private async void VolverInicioClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//inicio");
    }
}
