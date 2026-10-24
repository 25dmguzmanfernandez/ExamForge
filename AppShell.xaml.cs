using ExamForge.Views;

namespace ExamForge;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Páginas fuera de la TabBar. Se abren con Shell.Current.GoToAsync(nameof(Pagina))
        Routing.RegisterRoute(nameof(NuevaPreguntaPage), typeof(NuevaPreguntaPage));
        Routing.RegisterRoute(nameof(CuestionarioPage), typeof(CuestionarioPage));
    }
}
