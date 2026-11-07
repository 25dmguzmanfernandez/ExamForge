using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private string nombre = "";
    [ObservableProperty] private string apellidos = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string confirmarPassword = "";
    [ObservableProperty] private string mensajeError = "";
    [ObservableProperty] private string mensajeInfo = "";
    [ObservableProperty] private string modo = "Elegir";

    public LoginViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private void IrLogin()
    {
        LimpiarMensajes();
        Modo = "Login";
    }

    [RelayCommand]
    private void IrRegistro()
    {
        LimpiarMensajes();
        Modo = "Registro";
    }

    [RelayCommand]
    private void Volver()
    {
        LimpiarMensajes();
        
        Modo = "Elegir";
    }

    [RelayCommand]
    private async Task ProcesarAutenticacion()
    {
        LimpiarMensajes();

        if (modo == "Registro")
        {
            Registrar();
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrEmpty(Password))
        {
            MensajeError = "Introduce el usuario y la contraseña.";
            return;
        }

        var usuario = _dataService.ObtenerUsuarios().FirstOrDefault(u =>
            u.Nombre.Equals(Nombre.Trim(), StringComparison.OrdinalIgnoreCase));

        if (usuario is null || !VerificarPassword(Password, usuario.Password))
        {
            MensajeError = "Usuario o contraseña incorrectos.";
            return;
        }

        // Actualiza contraseñas antiguas en texto plano al formato hash.
        if (usuario.Password == Password)
        {
            usuario.Password = PasswordHasher.Crear(Password);
            _dataService.GuardarDatos();
        }

        _dataService.UsuarioActivo = usuario;
        await Shell.Current.GoToAsync("//inicio");
    }

    private void Registrar()
    {
        string nombreLimpio = Nombre.Trim();
        string apellidosLimpios = Apellidos.Trim();
        string correoLimpio = Email.Trim();

        if (string.IsNullOrWhiteSpace(nombreLimpio) ||
            string.IsNullOrWhiteSpace(apellidosLimpios) ||
            string.IsNullOrWhiteSpace(correoLimpio) ||
            string.IsNullOrEmpty(Password) ||
            string.IsNullOrEmpty(ConfirmarPassword))
        {
            MensajeError = "Rellena todos los campos.";
            return;
        }

        if (!Validar.TextoSeguro(nombreLimpio) ||
            !Validar.TextoSeguro(apellidosLimpios) ||
            !Validar.TextoSeguro(correoLimpio))
        {
            MensajeError = "El texto contiene caracteres no permitidos (< >).";
            return;
        }

        if (!Validar.CorreoValido(correoLimpio))
        {
            MensajeError = "El correo no es válido.";
            return;
        }

        if (Password.Length < 6)
        {
            MensajeError = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        if (Password != ConfirmarPassword)
        {
            MensajeError = "Las contraseñas no coinciden.";
            return;
        }

        var usuarios = _dataService.ObtenerUsuarios();

        if (usuarios.Any(u => u.Nombre.Equals(nombreLimpio, StringComparison.OrdinalIgnoreCase)))
        {
            MensajeError = "Ese nombre de usuario ya está registrado.";
            return;
        }

        if (usuarios.Any(u => u.Email.Equals(correoLimpio, StringComparison.OrdinalIgnoreCase)))
        {
            MensajeError = "Ese correo ya está registrado.";
            return;
        }

        usuarios.Add(new Usuario
        {
            Nombre = nombreLimpio,
            Apellidos = apellidosLimpios,
            Email = correoLimpio,
            Password = PasswordHasher.Crear(Password)
        });

        _dataService.GuardarDatos();

        Nombre = nombreLimpio;
        Apellidos = "";
        Email = "";
        Password = "";
        ConfirmarPassword = "";
        MensajeInfo = "Usuario creado. Ahora inicia sesión.";
        Modo = "Login";
    }

    private static bool VerificarPassword(string introducida, string guardada)
    {
        // Permite acceder a cuentas antiguas que aún tengan contraseña en texto plano.
        if (guardada == introducida)
            return true;

        return guardada == PasswordHasher.Crear(introducida);
    }

    private void LimpiarMensajes()
    {
        MensajeError = "";
        MensajeInfo = "";
    }
}