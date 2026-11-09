/*
 * AsignaturasPage: recibe cursoId y muestra las asignaturas de ese curso.
 * La página conserva el estado visual de las tarjetas; AsignaturasViewModel
 * carga y modifica los datos.
 */
using System.Collections.ObjectModel;
using ExamForge.Models;
using ExamForge.Views.Componentes;
using ExamForge.Views.Vistas;
using ExamForge.ViewModels;

namespace ExamForge.Views;

[QueryProperty(nameof(CursoId), "cursoId")]
public partial class AsignaturasPage : ContentPage
{
    private readonly AsignaturasViewModel vm;
    private readonly ObservableCollection<AsignaturaVista> asignaturas = new();

    private AsignaturaVista? enEdicion;
    private AsignaturaVista? destinoTema;

    public string CursoId { get; set; } = "";

    public AsignaturasPage(AsignaturasViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;
        ListaAsignaturas.ItemsSource = asignaturas;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        vm.Inicializar(CursoId);

        var curso = vm.CursoActual;
        if (curso is null) return;

        CursoSiglasLabel.Text = curso.Siglas;
        CursoNombreLabel.Text = curso.Nombre;
        CursoDescripcionLabel.Text = curso.Descripcion;

        Recargar();
    }

    private void Recargar()
    {
        var abiertas = asignaturas
            .Where(a => a.Desplegado)
            .Select(a => a.Modelo.Id)
            .ToHashSet();

        asignaturas.Clear();

        foreach (var asignatura in vm.Asignaturas)
        {
            asignaturas.Add(new AsignaturaVista(asignatura)
            {
                Desplegado = abiertas.Contains(asignatura.Id)
            });
        }

        ActualizarBotonEliminar();
    }

    private static T? Contexto<T>(object sender) where T : class =>
        (sender as BindableObject)?.BindingContext as T;

    private void SeleccionCambiada(object sender, CheckedChangedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is AsignaturaVista vista)
            vista.Seleccionado = e.Value;

        ActualizarBotonEliminar();
    }

    private void ActualizarBotonEliminar() =>
        EliminarSeleccionButton.IsVisible = asignaturas.Any(a => a.Seleccionado);

    private async void AtrasClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private void PerfilClicked(object sender, EventArgs e) => Perfil.Abrir();

    // ---------- Crear / editar asignatura ----------
    private void AnadirClicked(object sender, EventArgs e)
    {
        enEdicion = null;
        PopupAsignatura.Encabezado = "Nueva asignatura";
        PopupAsignatura.TextoConfirmar = "Agregar asignatura";
        PopupAsignatura.Abrir();
    }

    private void EditarClicked(object sender, EventArgs e)
    {
        enEdicion = Contexto<AsignaturaVista>(sender);
        if (enEdicion == null) return;

        PopupAsignatura.Encabezado = "Editar asignatura";
        PopupAsignatura.TextoConfirmar = "Guardar";
        PopupAsignatura.Abrir(enEdicion.Nombre, enEdicion.Descripcion);
    }

    private void AsignaturaConfirmada(object sender, FormularioEventArgs e)
    {
        if (enEdicion is not null)
            vm.EditarAsignatura(enEdicion.Modelo.Id, e.Titulo, e.Descripcion);
        else
            vm.AgregarAsignatura(e.Titulo, e.Descripcion);

        Recargar();
    }

    // ---------- Temas ----------
    private void AnadirTemaClicked(object sender, EventArgs e)
    {
        destinoTema = Contexto<AsignaturaVista>(sender);
        if (destinoTema != null) PopupTema.Abrir();
    }

    private void TemaConfirmado(object sender, FormularioEventArgs e)
    {
        if (destinoTema is null) return;

        vm.AgregarTema(destinoTema.Modelo.Id, e.Titulo);
        Recargar();
    }

    private async void TemaClicked(object sender, EventArgs e)
    {
        var tema = Contexto<TemaVista>(sender);
        if (tema == null) return;

        await Shell.Current.GoToAsync(nameof(BancoPreguntasPage), new Dictionary<string, object>
        {
            { "temaId", tema.Modelo.Id }
        });
    }

    private async void CuestionarioClicked(object sender, EventArgs e)
    {
        var vista = Contexto<AsignaturaVista>(sender);
        if (vista == null) return;

        await Shell.Current.GoToAsync(
                nameof(ConfigurarCuestionarioPage),
                new Dictionary<string, object>
                {
                    ["asignaturaId"] = vista.Modelo.Id
                });
    }

    // ---------- Eliminar ----------
    private async void EliminarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<AsignaturaVista>(sender);
        if (vista == null || vm.CursoActual == null) return;

        bool ok = await DisplayAlertAsync("Eliminar asignatura", $"¿Eliminar «{vista.Nombre}» y todo su contenido?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarAsignaturas(new[] { vista.Modelo.Id });
        Recargar();
    }

    // ---------- Eliminación múltiple ----------
    private async void EliminarSeleccionClicked(object sender, EventArgs e)
    {
        if (vm.CursoActual == null) return;

        var elegidas = asignaturas.Where(a => a.Seleccionado).ToList();
        if (elegidas.Count == 0)
        {
            await DisplayAlertAsync("Eliminar", "Selecciona al menos una asignatura.", "OK");
            return;
        }

        bool ok = await DisplayAlertAsync("Eliminar asignaturas", $"¿Eliminar {elegidas.Count} asignatura(s) y todo su contenido?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarAsignaturas(elegidas.Select(a => a.Modelo.Id));
        Recargar();
    }

    // ---------- Card ----------
    private void DesplegarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<AsignaturaVista>(sender);
        if (vista != null) vista.Desplegado = !vista.Desplegado;
    }

}
