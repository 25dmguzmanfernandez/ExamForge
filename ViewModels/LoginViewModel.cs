using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty] private string email;
        [ObservableProperty] private string nombre;
        [ObservableProperty] private string password;
        [ObservableProperty] private bool esRegistro;
        [ObservableProperty] private string textoBotonPrincipal = "Iniciar Sesión";
        [ObservableProperty] private string textoBotonAlternativo = "¿No tienes cuenta? Regístrate";

        public LoginViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        private void AlternarModo()
        {
            EsRegistro = !EsRegistro;
            TextoBotonPrincipal = EsRegistro ? "Crear Cuenta" : "Iniciar Sesión";
            TextoBotonAlternativo = EsRegistro ? "¿Ya tienes cuenta? Inicia Sesión" : "¿No tienes cuenta? Regístrate";

            Password = string.Empty;
        }

        [RelayCommand]
        private async Task ProcesarAutenticacion()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)) return;

            var usuarios = _dataService.ObtenerUsuarios();
            Usuario usuarioLogueado = null;

            if (EsRegistro)
            {
                if (string.IsNullOrWhiteSpace(Nombre)) return;

                if (usuarios.Any(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase))) return;

                usuarioLogueado = new Usuario
                {
                    Nombre = Nombre,
                    Email = Email,
                    Password = Password
                };
                usuarios.Add(usuarioLogueado);
                _dataService.GuardarDatos();
            }
            else
            {
                usuarioLogueado = usuarios.FirstOrDefault(u =>
                    u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == Password); 

                if (usuarioLogueado == null) return; 
            }

        }
    }
}
