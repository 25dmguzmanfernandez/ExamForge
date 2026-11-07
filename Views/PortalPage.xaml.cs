using ExamForge.ViewModels;

namespace ExamForge.Views;

public partial class PortalPage : ContentPage
{
    private readonly LoginViewModel vm;

    public PortalPage(LoginViewModel vm)
    {
        InitializeComponent();

        this.vm = vm;
        BindingContext = vm;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.Modo))
                CambiarModo(vm.Modo);

            if (e.PropertyName == nameof(LoginViewModel.MensajeError))
                ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(vm.MensajeError);

            if (e.PropertyName == nameof(LoginViewModel.MensajeInfo))
                InfoLabel.IsVisible = !string.IsNullOrWhiteSpace(vm.MensajeInfo);
        };

        CambiarModo(vm.Modo);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Al volver al Portal, por ejemplo después de cerrar sesión,
        // empieza mostrando las opciones iniciales.
        vm.VolverCommand.Execute(null);
    }

    private void CambiarModo(string nuevoModo)
    {
        bool elegir = nuevoModo == "Elegir";
        bool registro = nuevoModo == "Registro";

        ElegirLayout.IsVisible = elegir;
        FormularioBorder.IsVisible = !elegir;

        NombreEntry.IsVisible = !elegir;
        ApellidosEntry.IsVisible = registro;
        CorreoEntry.IsVisible = registro;
        ConfirmarEntry.IsVisible = registro;

        VolverButton.IsVisible = !elegir;
        AccionButton.IsVisible = !elegir;
        AccionButton.Text = registro ? "Crear usuario" : "Entrar";

        TituloLabel.Text = nuevoModo switch
        {
            "Registro" => "Crea tu usuario",
            "Login" => "Iniciar sesión",
            _ => "Bienvenido a ExamForge"
        };
    }
}