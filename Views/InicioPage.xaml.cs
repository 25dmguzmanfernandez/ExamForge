/*
 * InicioPage.xaml.cs: ventana 2 (portal de cursos).
 * Recargar: reconstruye la lista de CursoVista desde el usuario activo (se llama en OnAppearing y tras cada cambio).
 * AnadirClicked / EditarClicked / CursoConfirmado: popup de curso (crear o editar). EliminarClicked: borra uno con confirmación.
 * SeleccionarClicked / EliminarSeleccionClicked: modo selección múltiple. CursoTapped: abre el curso (o marca la casilla).
 */
using ExamForge.Models;
using ExamForge.ViewModels;
using ExamForge.Views.Componentes;
using ExamForge.Views.Vistas;
using System.Collections.ObjectModel;

namespace ExamForge.Views;

public partial class InicioPage : ContentPage
{
    private readonly ObservableCollection<CursoVista> cursos = new();
    private bool enSeleccion;
    private CursoVista? enEdicion;

    public InicioPage()
    {
        InitializeComponent();
        ListaCursos.ItemsSource = cursos;
    }

    protected override void OnAppearing()
    {
        vm.Inicializar();
        ActualizarBienvenida();
        Recargar();
    }

    private void ActualizarBienvenida() =>
    BienvenidaLabel.Text = $"¡Hola, {vm.UsuarioActual?.Nombre ?? ""}!";

    private void Recargar()
    {
        cursos.Clear();

        foreach (Curso curso in vm.Cursos)
            cursos.Add(new CursoVista(curso) { EnSeleccion = enSeleccion });
    }

    private static T? Contexto<T>(object sender) where T : class =>
        (sender as BindableObject)?.BindingContext as T;

    // ---------- Perfil ----------
    private void PerfilClicked(object sender, EventArgs e) => Perfil.Abrir();

    private void PerfilGuardado(object sender, EventArgs e) => ActualizarBienvenida();

    // ---------- Crear / editar ----------
    private void AnadirClicked(object sender, EventArgs e)
    {
        enEdicion = null;
        PopupCurso.Encabezado = "Nuevo curso";
        PopupCurso.TextoConfirmar = "Agregar curso";
        PopupCurso.Abrir();
    }

    private readonly HomeViewModel vm;

    public InicioPage(HomeViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;
        ListaCursos.ItemsSource = cursos;
    }

    private void EditarClicked(object sender, EventArgs e)
    {
        enEdicion = Contexto<CursoVista>(sender);
        if (enEdicion == null) return;

        PopupCurso.Encabezado = "Editar curso";
        PopupCurso.TextoConfirmar = "Guardar";
        PopupCurso.Abrir(enEdicion.Nombre, enEdicion.Descripcion);
    }

    private void CursoConfirmado(object sender, FormularioEventArgs e)
    {
        if (enEdicion is not null)
            vm.EditarCurso(enEdicion.Modelo.Id, e.Titulo, e.Descripcion);
        else
            vm.AgregarCurso(e.Titulo, e.Descripcion);

        Recargar();
    }

    // ---------- Eliminar ----------
    private async void EliminarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<CursoVista>(sender);
        if (vista == null) return;

        bool ok = await DisplayAlertAsync("Eliminar curso", $"¿Eliminar «{vista.Nombre}» y todo su contenido?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarCursos(new[] { vista.Modelo.Id });
        Recargar();
    }

    // ---------- Selección múltiple ----------
    private void SeleccionarClicked(object sender, EventArgs e)
    {
        enSeleccion = !enSeleccion;
        SeleccionarButton.Text = enSeleccion ? "Cancelar" : "Seleccionar";
        EliminarSeleccionButton.IsVisible = enSeleccion;

        foreach (var vista in cursos)
        {
            vista.EnSeleccion = enSeleccion;
            vista.Seleccionado = false;
        }
    }

    private async void EliminarSeleccionClicked(object sender, EventArgs e)
    {
        var elegidos = cursos.Where(c => c.Seleccionado).ToList();
        if (elegidos.Count == 0)
        {
            await DisplayAlertAsync("Eliminar", "Selecciona al menos un curso.", "OK");
            return;
        }

        bool ok = await DisplayAlertAsync("Eliminar cursos", $"¿Eliminar {elegidos.Count} curso(s) y todo su contenido?", "Eliminar", "Cancelar");
        if (!ok) return;

        vm.EliminarCursos(elegidos.Select(c => c.Modelo.Id));
        SeleccionarClicked(sender, e);
        Recargar();
    }

    // ---------- Card ----------
    private void DesplegarClicked(object sender, EventArgs e)
    {
        var vista = Contexto<CursoVista>(sender);
        if (vista != null) vista.Desplegado = !vista.Desplegado;
    }

    private async void CursoTapped(object sender, TappedEventArgs e)
    {
        var vista = Contexto<CursoVista>(sender);
        if (vista == null) return;

        if (enSeleccion)
        {
            vista.Seleccionado = !vista.Seleccionado;
            return;
        }

        await Shell.Current.GoToAsync(nameof(AsignaturasPage),
    new Dictionary<string, object>
    {
        ["cursoId"] = vista.Modelo.Id
    });
    }
}
