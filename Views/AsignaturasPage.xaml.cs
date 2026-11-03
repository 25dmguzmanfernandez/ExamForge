namespace ExamForge.Views;

public partial class AsignaturasPage : ContentPage
{
    // Cuando existan modelo y persistencia: private readonly GestorAsignaturas gestor;
    // y constructor con inyección (registrar la página como AddTransient en MauiProgram).
    private object? asignaturaActual;

    public AsignaturasPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // TODO: ListaAsignaturas.ItemsSource = gestor.Asignaturas;
    }

    private void AsignaturaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        asignaturaActual = e.CurrentSelection.FirstOrDefault();

        if (asignaturaActual == null)
        {
            TemasLabel.Text = "Temas";
            ListaTemas.ItemsSource = null;
            return;
        }

        // TODO: TemasLabel.Text = $"Temas de {asignatura.Nombre}";
        // TODO: ListaTemas.ItemsSource = asignatura.Temas;
    }

    private async void AgregarAsignaturaClicked(object sender, EventArgs e)
    {
        string nombre = NombreEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(nombre))
        {
            await DisplayAlertAsync("Aviso", "Escribe el nombre de la asignatura", "OK");
            return;
        }

        // TODO: gestor.AgregarAsignatura(new Asignatura(nombre)); + guardar
        NombreEntry.Text = "";
    }

    private async void AgregarTemaClicked(object sender, EventArgs e)
    {
        if (asignaturaActual == null)
        {
            await DisplayAlertAsync("Aviso", "Selecciona primero una asignatura", "OK");
            return;
        }

        string nombre = NombreEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(nombre))
        {
            await DisplayAlertAsync("Aviso", "Escribe el nombre del tema", "OK");
            return;
        }

        // TODO: asignatura.AddTema(new Tema(nombre)); + guardar
        NombreEntry.Text = "";
    }
}
