/*
 * FormularioPopup.xaml.cs: lógica visual del popup título + descripción.
 * Propiedades: Encabezado, TextoConfirmar, PlaceholderTitulo, MostrarDescripcion (se ponen desde XAML o código).
 * Abrir(titulo, descripcion): lo muestra (vacío para crear, con datos para editar). Cerrar(): lo oculta.
 * Evento Confirmado: se lanza con los datos ya validados (obligatorio y sin código inyectado); la página decide qué hacer.
 */
using ExamForge.Services;

namespace ExamForge.Views.Componentes;

public class FormularioEventArgs : EventArgs
{
    public string Titulo { get; }
    public string Descripcion { get; }

    public FormularioEventArgs(string titulo, string descripcion)
    {
        Titulo = titulo;
        Descripcion = descripcion;
    }
}

public partial class FormularioPopup : ContentView
{
    public event EventHandler<FormularioEventArgs>? Confirmado;

    public FormularioPopup()
    {
        InitializeComponent();
    }

    public string Encabezado { get => EncabezadoLabel.Text; set => EncabezadoLabel.Text = value; }
    public string TextoConfirmar { get => ConfirmarButton.Text; set => ConfirmarButton.Text = value; }
    public string PlaceholderTitulo { get => TituloEntry.Placeholder; set => TituloEntry.Placeholder = value; }
    public bool MostrarDescripcion { get => DescripcionEditor.IsVisible; set => DescripcionEditor.IsVisible = value; }

    public void Abrir(string titulo = "", string descripcion = "")
    {
        TituloEntry.Text = titulo;
        DescripcionEditor.Text = descripcion;
        ErrorLabel.IsVisible = false;
        IsVisible = true;
    }

    public void Cerrar() => IsVisible = false;

    private void OnVeloTapped(object sender, TappedEventArgs e)
    {
        // Intencionadamente vacío: solo absorbe los toques.
    }

    private void OnCancelar(object sender, EventArgs e) => Cerrar();

    private void OnConfirmar(object sender, EventArgs e)
    {
        string titulo = TituloEntry.Text?.Trim() ?? string.Empty;
        string descripcion = DescripcionEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(titulo))
        {
            Error("El título es obligatorio.");
            return;
        }

        if (!Validar.TextoSeguro(titulo) || !Validar.TextoSeguro(descripcion))
        {
            Error("El texto contiene caracteres no permitidos (< >).");
            return;
        }

        Cerrar();
        Confirmado?.Invoke(this, new FormularioEventArgs(titulo, descripcion));
    }

    private void Error(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
