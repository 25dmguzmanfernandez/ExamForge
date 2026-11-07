/*
 * BancoPreguntasPage.xaml.cs: ventana 4 (Tema). Recibe "asignatura" y "tema" por QueryProperty.
 * Pestañas: MostrarPestana(banco) alterna el banco de preguntas y el historial de exámenes.
 * Banco: Recargar, crear/editar (abre NuevaPreguntaPage), eliminar con confirmación y selección múltiple.
 * Historial: ExamenVista (puntuación N/total, verde si correctas > total/2); tocar uno abre CuestionarioPage en modo revisión.
 * CuestionarioClicked: abre ConfigurarCuestionarioPage con la asignatura y este tema fijado.
 * TODO controlador: Provisional.EliminarPreguntas y la lectura de preguntas/exámenes se sustituyen por el controlador.
 */
using System.Collections.ObjectModel;
using ExamForge.Models;
using ExamForge.Views.Componentes;
using ExamForge.Views.Vistas;
using ExamForge.ViewModels;

namespace ExamForge.Views;

[QueryProperty(nameof(TemaId), "temaId")]
public partial class BancoPreguntasPage : ContentPage
{
    private readonly TemaViewModel vm;

    public string TemaId { get; set; } = "";
    private readonly ObservableCollection<PreguntaVista> preguntas = new();
    private readonly ObservableCollection<ExamenVista> examenes = new();
    private bool enSeleccion;

    public BancoPreguntasPage(TemaViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;

        ListaPreguntas.ItemsSource = preguntas;
        ListaExamenes.ItemsSource = examenes;
        MostrarPestana(true);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        vm.Inicializar(TemaId);

        if (vm.TemaActual is null || vm.AsignaturaActual is null) return;

        AsignaturaSiglasLabel.Text = vm.AsignaturaActual.Siglas;
        AsignaturaNombreLabel.Text = vm.AsignaturaActual.Nombre;
        TemaNombreLabel.Text = vm.NombreTema;

        Recargar();
    }

    private void Recargar()
    {
        preguntas.Clear();

        foreach (var pregunta in vm.BancoDePreguntas)
            preguntas.Add(PreguntaVista.ParaBanco(pregunta));

        foreach (var vista in preguntas)
            vista.EnSeleccion = enSeleccion;

        TotalPreguntasLabel.Text = $"{preguntas.Count} pregunta(s)";

        examenes.Clear();

        if (vm.TemaActual is null) return;

        var historial = vm.Cuestionarios
            .OrderBy(c => c.FechaCreacion)
            .Select((cuestionario, i) => new ExamenVista(cuestionario, i + 1, vm.TemaActual))
            .Reverse();

        foreach (var examen in historial)
            examenes.Add(examen);
    }

    private static T? Contexto<T>(object sender) where T : class =>
        (sender as BindableObject)?.BindingContext as T;

    private async void AtrasClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private void PerfilClicked(object sender, EventArgs e) => Perfil.Abrir();

    // ---------- Pestañas ----------
    private void PestanaBancoClicked(object sender, EventArgs e) => MostrarPestana(true);

    private void PestanaHistorialClicked(object sender, EventArgs e) => MostrarPestana(false);

    private void MostrarPestana(bool banco)
    {
        BancoCabeceraGrid.IsVisible = banco;
        ListaPreguntas.IsVisible = banco;
        ListaExamenes.IsVisible = !banco;

        Estilizar(PestanaBancoButton, banco);
        Estilizar(PestanaHistorialButton, !banco);
    }

    private static void Estilizar(Button boton, bool activa)
    {
        boton.BackgroundColor = activa ? Paleta.AzulOscuro : Colors.Transparent;
        boton.TextColor = activa ? Colors.White : Paleta.AzulOscuro;
    }

    // ---------- Banco: crear / editar / eliminar ----------
    private async void AnadirClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage),
            new Dictionary<string, object>
            {
                ["temaId"] = vm.TemaActual!.Id
            });
    }

    private async void EditarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<PreguntaVista>(sender);
        if (vista == null) return;

        await Shell.Current.GoToAsync(nameof(NuevaPreguntaPage),
    new Dictionary<string, object>
    {
        ["temaId"] = vm.TemaActual!.Id,
        ["preguntaId"] = vista.Modelo.Id
    });
    }

    private async void EliminarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<PreguntaVista>(sender);
        if (vista == null || vm.TemaActual == null) return;

        bool ok = await DisplayAlertAsync("Eliminar pregunta", "¿Eliminar esta pregunta del banco?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarPreguntas(new[] { vista.Modelo.Id });
        Recargar();
    }

    // ---------- Banco: selección múltiple ----------
    private void SeleccionarClicked(object sender, EventArgs e)
    {
        enSeleccion = !enSeleccion;
        SeleccionarButton.Text = enSeleccion ? "Cancelar" : "Seleccionar";
        EliminarSeleccionButton.IsVisible = enSeleccion;

        foreach (var vista in preguntas)
        {
            vista.EnSeleccion = enSeleccion;
            vista.Seleccionado = false;
        }
    }

    private async void EliminarSeleccionClicked(object sender, EventArgs e)
    {
        if (vm.TemaActual == null) return;

        var elegidas = preguntas.Where(p => p.Seleccionado).ToList();
        if (elegidas.Count == 0)
        {
            await DisplayAlertAsync("Eliminar", "Selecciona al menos una pregunta.", "OK");
            return;
        }

        bool ok = await DisplayAlertAsync("Eliminar preguntas", $"¿Eliminar {elegidas.Count} pregunta(s)?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarPreguntas(elegidas.Select(p => p.Modelo.Id));
        SeleccionarClicked(sender, e);
        Recargar();
    }

    private void DesplegarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<PreguntaVista>(sender);
        if (vista != null) vista.Desplegado = !vista.Desplegado;
    }

    private void PreguntaTapped(object sender, TappedEventArgs e)
    {
        var vista = Contexto<PreguntaVista>(sender);
        if (vista != null && enSeleccion) vista.Seleccionado = !vista.Seleccionado;
    }

    // ---------- Historial ----------
    private async void ExamenTapped(object sender, TappedEventArgs e)
    {
        var examen = Contexto<ExamenVista>(sender);
        if (examen == null) return;

        await Shell.Current.GoToAsync(
            nameof(CuestionarioPage),
            new Dictionary<string, object>
            {
                ["examenId"] = examen.Modelo.Id
            });
    }

    // ---------- Cuestionario ----------
    private async void CuestionarioClicked(object sender, EventArgs e)
    {
        if (vm.AsignaturaActual is null || vm.TemaActual is null) return;

        if (vm.AsignaturaActual is null || vm.TemaActual is null) return;

        await Shell.Current.GoToAsync(
            nameof(ConfigurarCuestionarioPage),
            new Dictionary<string, object>
            {
                ["asignaturaId"] = vm.AsignaturaActual.Id,
                ["temaId"] = vm.TemaActual.Id
            });
    }
}
