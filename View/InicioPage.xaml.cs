namespace ExamForge.Views;

public partial class InicioPage : ContentPage
{
    public InicioPage()
    {
        InitializeComponent();
    }

    private async void AsignaturasClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//asignaturas");
    }

    private async void BancoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//banco");
    }

    private async void EmpezarCuestionarioClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CuestionarioPage));
    }
}
