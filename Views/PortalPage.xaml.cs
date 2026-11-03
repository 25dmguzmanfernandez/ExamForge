using System.Security.Cryptography;
using System.Text;

namespace ExamForge.Views;

public partial class PortalPage : ContentPage
{
    // true = ya hay usuario creado (solo nombre y contraseña); false = primera vez (registro)
    private bool usuarioCreado = false;

    public PortalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        NombreEntry.Text = "";
        ApellidoEntry.Text = "";
        CorreoEntry.Text = "";
        ClaveEntry.Text = "";
        ErrorLabel.IsVisible = false;

        //BORRAR DESPUES
        usuarioCreado = Preferences.Default.ContainsKey("usuario_nombre");

        ApellidoEntry.IsVisible = !usuarioCreado;
        CorreoEntry.IsVisible = !usuarioCreado;
        TituloLabel.Text = usuarioCreado ? "Introduce tus credenciales" : "Crea tu usuario para empezar";
        AccederButton.Text = usuarioCreado ? "Entrar" : "Crear usuario";
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }

    private async void AccederClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        string nombre = NombreEntry.Text?.Trim() ?? string.Empty;
        string clave = ClaveEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(clave))
        {
            MostrarError("Introduce nombre y contraseña.");
            return;
        }

        //BORRAR DESPUES
        if (!usuarioCreado)
        {
            string apellido = ApellidoEntry.Text?.Trim() ?? string.Empty;
            string correo = CorreoEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(correo))
            {
                MostrarError("Rellena todos los campos.");
                return;
            }

            if (!correo.Contains('@') || !correo.Contains('.'))
            {
                MostrarError("El correo no es válido.");
                return;
            }

            Preferences.Default.Set("usuario_nombre", nombre);
            Preferences.Default.Set("usuario_apellido", apellido);
            Preferences.Default.Set("usuario_correo", correo);
            Preferences.Default.Set("usuario_clave", Hash(clave));
        }
        else
        {
            string nombreGuardado = Preferences.Default.Get("usuario_nombre", "");
            string claveGuardada = Preferences.Default.Get("usuario_clave", "");

            if (!string.Equals(nombre, nombreGuardado, StringComparison.OrdinalIgnoreCase)
                || Hash(clave) != claveGuardada)
            {
                MostrarError("Usuario o contraseña incorrectos.");
                return;
            }
        }

        await Shell.Current.GoToAsync("//inicio");
    }

    //BORRAR DESPUES
    private static string Hash(string texto)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
    }
}
