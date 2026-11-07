using ExamForge.ViewModels;
using ExamForge.Views.Componentes;
using ExamForge.Views.Vistas;
using ExamForge.Models;

namespace ExamForge.Views;

[QueryProperty(nameof(ExamenId), "examenId")]
public partial class CuestionarioPage : ContentPage
{
    private readonly CuestionarioViewModel vm;
    private readonly List<PreguntaVista> preguntas = new();
    private bool cargada;
    private bool finalizado;

    public string? ExamenId { get; set; }

    public CuestionarioPage(CuestionarioViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;
        BindableLayout.SetItemsSource(PreguntasLayout, preguntas);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (cargada) return;

        vm.Inicializar(ExamenId);
        var cuestionario = vm.CuestionarioActual;

        if (cuestionario is null)
        {
            await DisplayAlertAsync("Cuestionario", "No se encontró el cuestionario.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        cargada = true;

        for (int i = 0; i < cuestionario.Preguntas.Count; i++)
        {
            var pregunta = cuestionario.Preguntas[i];

            preguntas.Add(vm.EsRevision
                ? PreguntaVista.ParaRevision(pregunta, cuestionario, i + 1)
                : PreguntaVista.ParaCuestionario(pregunta, i + 1));
        }

        BindableLayout.SetItemsSource(PreguntasLayout, null);
        BindableLayout.SetItemsSource(PreguntasLayout, preguntas);

        FinalizarButton.IsVisible = !vm.EsRevision;
        TituloLabel.Text = vm.EsRevision
            ? $"Revisión · {cuestionario.FechaCreacion:dd/MM/yyyy HH:mm} · {cuestionario.Aciertos} / {cuestionario.TotalPreguntas}"
            : $"Cuestionario · {cuestionario.TotalPreguntas} preguntas";
    }

    private void OnVeloTapped(object sender, TappedEventArgs e)
    {
        // Absorbe el toque sobre el fondo del panel de resultado.
    }

    private async void AtrasClicked(object sender, EventArgs e) =>
        await SalirAsync();

    protected override bool OnBackButtonPressed()
    {
        _ = SalirAsync();
        return true;
    }

    private async Task SalirAsync()
    {
        if (!vm.EsRevision && !finalizado)
        {
            bool salir = await DisplayAlertAsync(
                "Salir del cuestionario",
                "Si sales ahora se borrará este intento y no podrás reanudarlo.",
                "Salir",
                "Seguir");

            if (!salir) return;

            vm.CancelarIntento();
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void FinalizarClicked(object sender, EventArgs e)
    {
        var cuestionario = vm.CuestionarioActual;
        if (cuestionario is null || vm.EsRevision) return;

        int sinResponder = preguntas.Count(
            p => p.Opciones.All(o => !o.Seleccionada));

        string mensaje = sinResponder > 0
            ? $"Tienes {sinResponder} pregunta(s) sin responder. ¿Finalizar el cuestionario?"
            : "¿Finalizar el cuestionario?";

        bool confirmar = await DisplayAlertAsync(
            "Finalizar", mensaje, "Finalizar", "Seguir");

        if (!confirmar) return;

        var respuestas = new Dictionary<string, int>();

        foreach (var pregunta in preguntas)
        {
            int marcada = pregunta.Opciones.FindIndex(o => o.Seleccionada);
            if (marcada >= 0)
                respuestas[pregunta.Modelo.Id] = marcada;
        }

        double nota = vm.Finalizar(respuestas);
        finalizado = true;
        MostrarResultado(cuestionario, nota);
    }

    private void MostrarResultado(Cuestionario cuestionario, double nota)
    {
        bool aprobado = cuestionario.Aciertos > cuestionario.TotalPreguntas / 2.0;

        AciertosLabel.Text =
            $"Correctas: {cuestionario.Aciertos} / {cuestionario.TotalPreguntas}";
        NotaLabel.Text = nota.ToString("0.0");
        NotaLabel.TextColor = aprobado ? Paleta.Verde : Paleta.Rojo;
        ResultadoOverlay.IsVisible = true;
    }

    private async void CerrarResultadoClicked(object sender, EventArgs e)
    {
        ResultadoOverlay.IsVisible = false;
        await Shell.Current.GoToAsync("..");
    }
}