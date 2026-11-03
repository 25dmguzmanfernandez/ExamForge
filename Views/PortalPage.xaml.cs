using ExamForge.Services;
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

        // Cargamos los datos una sola vez al entrar. Así nunca se crea o guarda
        // una pregunta antes de recuperar lo que había en el JSON.
        try
        {
            await DataStorage.CargarAsync();
            DataService.UsuarioActivo.Nombre = Preferences.Default.Get("usuario_nombre", nombre);
            DataService.UsuarioActivo.Email = Preferences.Default.Get("usuario_correo", "");
            await DataStorage.GuardarAsync();
            await Shell.Current.GoToAsync("//inicio");
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudieron recuperar los datos guardados: {ex.Message}");
        }
    }

    //BORRAR DESPUES
    private static string Hash(string texto)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
    }
}
