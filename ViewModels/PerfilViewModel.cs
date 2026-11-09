using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class PerfilViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string apellidos = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string passwordNueva = "";
    [ObservableProperty] private string confirmarPassword = "";
    [ObservableProperty] private string mensajeError = "";
    [ObservableProperty] private bool guardado;

    public PerfilViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Cargar()
    {
        MensajeError = "";
        Guardado = false;

        var usuario = _dataService.UsuarioActivo;
        if (usuario is null)
        {
            MensajeError = "No hay una sesión activa.";
            return;
        }

        Nombre = usuario.Nombre;
        Apellidos = usuario.Apellidos;
        Email = usuario.Email;
        PasswordNueva = "";
        ConfirmarPassword = "";
    }

    [RelayCommand]
    private void Guardar()
    {
        MensajeError = "";
        Guardado = false;

        var usuario = _dataService.UsuarioActivo;
        if (usuario is null)
        {
            MensajeError = "No hay una sesión activa.";
            return;
        }

        string nombreLimpio = Nombre.Trim();
        string apellidosLimpios = Apellidos.Trim();
        string emailLimpio = Email.Trim();

        if (nombreLimpio == "" || apellidosLimpios == "" || emailLimpio == "")
        {
            MensajeError = "Nombre, apellidos y correo son obligatorios.";
            return;
        }

        if (!Validar.TextoSeguro(nombreLimpio) ||
            !Validar.TextoSeguro(apellidosLimpios) ||
            !Validar.TextoSeguro(emailLimpio))
        {
            MensajeError = "El texto contiene caracteres no permitidos (< >).";
            return;
        }

        if (!Validar.CorreoValido(emailLimpio))
        {
            MensajeError = "El correo no es válido.";
            return;
        }

        bool emailEnUso = _dataService.ObtenerUsuarios().Any(u =>
            u.Id != usuario.Id &&
            u.Email.Equals(emailLimpio, StringComparison.OrdinalIgnoreCase));

        if (emailEnUso)
        {
            MensajeError = "Ese correo ya está registrado.";
            return;
        }

        if (!string.IsNullOrEmpty(PasswordNueva) &&
            (PasswordNueva.Length < 6 || PasswordNueva != ConfirmarPassword))
        {
            MensajeError = "La nueva contraseña debe tener al menos 6 caracteres y coincidir con la confirmación.";
            return;
        }

        usuario.Nombre = nombreLimpio;
        usuario.Apellidos = apellidosLimpios;
        usuario.Email = emailLimpio;

        if (!string.IsNullOrEmpty(PasswordNueva))
            usuario.Password = PasswordHasher.Crear(PasswordNueva);

        _dataService.GuardarDatos();
        Guardado = true;
    }
}