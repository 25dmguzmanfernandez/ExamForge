using ExamForge.ViewModels;

namespace ExamForge.Views;

[QueryProperty(nameof(AsignaturaId), "asignaturaId")]
[QueryProperty(nameof(TemaId), "temaId")]
public partial class ConfigurarCuestionarioPage : ContentPage
{
    private readonly ConfigurarTestViewModel vm;

    public string AsignaturaId { get; set; } = "";
    public string? TemaId { get; set; }

    public ConfigurarCuestionarioPage(ConfigurarTestViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ConfigurarTestViewModel.MensajeError))
                ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(vm.MensajeError);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        vm.Inicializar(AsignaturaId, TemaId);

        if (vm.TemaFijado && vm.TemasParaSeleccionar.FirstOrDefault() is { } tema)
        {
            OrigenLabel.Text = $"Tema: {tema.TemaRef.Nombre}";
            ListaTemas.IsVisible = false;
        }
        else
        {
            OrigenLabel.Text = $"Asignatura: {vm.NombreAsignatura}. Elige los temas:";
            ListaTemas.IsVisible = true;
        }

        ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(vm.MensajeError);
    }

    private async void CancelarClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}