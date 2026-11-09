using System.ComponentModel;
using ExamForge.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExamForge.Views.Componentes;

public partial class PerfilPopup : ContentView
{
    private PerfilViewModel? vm;

    public event EventHandler? Guardado;

    public PerfilPopup()
    {
        InitializeComponent();
    }

    public void Abrir()
    {
        if (vm is null)
        {
            vm = Handler?.MauiContext?.Services.GetService<PerfilViewModel>();
            if (vm is null) return;

            BindingContext = vm;
            vm.PropertyChanged += VmPropertyChanged;
        }

        vm.Cargar();
        IsVisible = true;
    }

    private void VmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PerfilViewModel.MensajeError))
            ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(vm?.MensajeError);

        if (e.PropertyName == nameof(PerfilViewModel.Guardado) && vm?.Guardado == true)
        {
            IsVisible = false;
            Guardado?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnVeloTapped(object sender, TappedEventArgs e)
    {
        // Absorbe los toques sobre el fondo del popup.
    }

    private void OnCerrar(object sender, EventArgs e) =>
        IsVisible = false;
}