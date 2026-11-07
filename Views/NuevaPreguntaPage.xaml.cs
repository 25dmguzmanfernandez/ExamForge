/*
 * NuevaPreguntaPage.xaml.cs: ventana 5 (crear o editar preguntas). Recibe "tema" y, al editar, "pregunta".
 * Manual: GuardarClicked valida enunciado y respuestas (2 a 4, sin vacías); la primera es la correcta (RespuestaCorrecta = 0).
 * Pestañas de generación (texto / PDF): visibles pero sin servicio todavía. ElegirPdfClicked usa FilePicker (incluido en MAUI)
 *   limitado a PDF. GenerarTextoClicked / GenerarArchivoClicked: TODO Ander (Gemini); cuando devuelva la lista basta llamar a MostrarGeneradas.
 * MostrarGeneradas / AnadirGeneradasClicked: selección múltiple (todas marcadas) de las preguntas generadas antes de guardarlas.
 * TODO controlador: Provisional.AgregarPregunta / EditarPregunta se sustituyen por el controlador de preguntas.
 */
using ExamForge.Models;
using ExamForge.ViewModels;
using ExamForge.Views.Componentes;
using ExamForge.Views.Vistas;
using System.Collections.ObjectModel;

namespace ExamForge.Views;

[QueryProperty(nameof(TemaId), "temaId")]
[QueryProperty(nameof(PreguntaId), "preguntaId")]

public partial class NuevaPreguntaPage : ContentPage
{
    private readonly NuevaPreguntaViewModel vm;
    private readonly ObservableCollection<PreguntaVista> generadas = new();
    private FileResult? archivoPdf;
    private int opcionesVisibles = 2;

    public string TemaId { get; set; } = "";
    public string? PreguntaId { get; set; }

    public NuevaPreguntaPage(NuevaPreguntaViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;
        ListaGeneradas.ItemsSource = generadas;
        MostrarPestana(1);

        vm.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(NuevaPreguntaViewModel.MensajeError))
                ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(vm.MensajeError);

            if (e.PropertyName == nameof(NuevaPreguntaViewModel.Guardado) && vm.Guardado)
                await Shell.Current.GoToAsync("..");
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        vm.Inicializar(TemaId, PreguntaId);
        opcionesVisibles = vm.CantidadOpciones;
        ActualizarOpciones();

        if (!string.IsNullOrWhiteSpace(PreguntaId))
        {
            TituloLabel.Text = "Editar pregunta";
            PestanasGrid.IsVisible = false;
            MostrarPestana(1);
        }
    }

    private async void AtrasClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private void OnVeloTapped(object sender, TappedEventArgs e)
    {
        // Intencionadamente vacío: solo absorbe los toques.
    }

    // ---------- Pestañas ----------
    private void PestanaManualClicked(object sender, EventArgs e) => MostrarPestana(1);
    private void PestanaTextoClicked(object sender, EventArgs e) => MostrarPestana(2);
    private void PestanaArchivoClicked(object sender, EventArgs e) => MostrarPestana(3);

    private void MostrarPestana(int numero)
    {
        ManualPanel.IsVisible = numero == 1;
        TextoPanel.IsVisible = numero == 2;
        ArchivoPanel.IsVisible = numero == 3;

        Estilizar(PestanaManualButton, numero == 1);
        Estilizar(PestanaTextoButton, numero == 2);
        Estilizar(PestanaArchivoButton, numero == 3);
    }

    private static void Estilizar(Button boton, bool activa)
    {
        boton.BackgroundColor = activa ? Paleta.AzulOscuro : Colors.Transparent;
        boton.TextColor = activa ? Colors.White : Paleta.AzulOscuro;
    }

    // ---------- Manual ----------
    private Entry[] Entradas => new[] { Opcion1Entry, Opcion2Entry, Opcion3Entry, Opcion4Entry };

    private void ActualizarOpciones()
    {
        var entradas = Entradas;
        for (int i = 0; i < entradas.Length; i++)
            entradas[i].IsVisible = i < opcionesVisibles;

        MasOpcionButton.IsVisible = opcionesVisibles < 4;
        MenosOpcionButton.IsVisible = opcionesVisibles > 2;
    }

    private void MasOpcionClicked(object sender, EventArgs e)
    {
        if (opcionesVisibles < 4)
        {
            opcionesVisibles++;
            vm.CantidadOpciones = opcionesVisibles;
        }

        ActualizarOpciones();
    }

    private void MenosOpcionClicked(object sender, EventArgs e)
    {
        if (opcionesVisibles <= 2) return;

        int indice = opcionesVisibles - 1;
        Entradas[indice].Text = "";

        if (indice == 2) vm.Opcion3 = "";
        if (indice == 3) vm.Opcion4 = "";

        opcionesVisibles--;
        vm.CantidadOpciones = opcionesVisibles;
        ActualizarOpciones();
    }




    // ---------- Generación (pendiente del servicio) ----------
    private async void ElegirPdfClicked(object sender, EventArgs e)
    {
        var resultado = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona un PDF",
            FileTypes = FilePickerFileType.Pdf
        });

        if (resultado == null) return; // el usuario canceló

        archivoPdf = resultado;
        ArchivoLabel.Text = resultado.FileName;
    }

    private async void GenerarTextoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PromptEditor.Text))
        {
            await DisplayAlertAsync("Aviso", "Escribe el texto del que generar preguntas.", "OK");
            return;
        }

        if (!int.TryParse(CantidadTextoEntry.Text, out int cantidad) || cantidad < 1)
        {
            await DisplayAlertAsync("Aviso", "Indica un número de preguntas válido.", "OK");
            return;
        }

        await GenerarAsync(null);
    }

    private async void GenerarArchivoClicked(object sender, EventArgs e)
    {
        if (archivoPdf == null)
        {
            await DisplayAlertAsync("Aviso", "Selecciona primero un archivo PDF.", "OK");
            return;
        }

        if (!int.TryParse(CantidadArchivoEntry.Text, out int cantidad) || cantidad < 1)
        {
            await DisplayAlertAsync("Aviso", "Indica un número de preguntas válido.", "OK");
            return;
        }

        await GenerarAsync(null);
    }

    private async Task GenerarAsync(List<Pregunta>? resultado)
    {
        CargandoIndicador.IsVisible = true;
        CargandoIndicador.IsRunning = true;
        try
        {
            // TODO Ander (feature/services): resultado = await servicio.GenerarDesdeTextoAsync(prompt, cantidad)
            //                                o GenerarDesdePdfAsync(archivoPdf, cantidad); debe devolver la correcta en la posición 0.
            if (resultado == null)
            {
                await DisplayAlertAsync("Aún no disponible", "La generación automática todavía no está conectada.", "OK");
                return;
            }

            MostrarGeneradas(resultado);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron generar las preguntas: {ex.Message}", "OK");
        }
        finally
        {
            CargandoIndicador.IsRunning = false;
            CargandoIndicador.IsVisible = false;
        }
    }

    /// <summary>Abre el panel para elegir cuáles de las preguntas generadas se añaden (todas marcadas).</summary>
    private void MostrarGeneradas(List<Pregunta> lista)
    {
        generadas.Clear();
        foreach (var pregunta in lista)
        {
            var vista = PreguntaVista.ParaBanco(pregunta);
            vista.Seleccionado = true;
            generadas.Add(vista);
        }
        GeneradasOverlay.IsVisible = true;
    }

    private void CancelarGeneradasClicked(object sender, EventArgs e) => GeneradasOverlay.IsVisible = false;

    private async void AnadirGeneradasClicked(object sender, EventArgs e)
    {
        vm.AgregarPreguntas(
            generadas
                .Where(g => g.Seleccionado)
                .Select(g => g.Modelo));

        GeneradasOverlay.IsVisible = false;
        await Shell.Current.GoToAsync("..");
    }
}
