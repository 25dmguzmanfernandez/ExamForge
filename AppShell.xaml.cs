using ExamForge.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ExamForge;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        var contenidos = Items
            .SelectMany(item => item.Items)
            .SelectMany(section => section.Items);

        var portal = contenidos.First(x => x.Route == "portal");
        portal.ContentTemplate =
            new DataTemplate(() => services.GetRequiredService<PortalPage>());

        var inicio = contenidos.First(x => x.Route == "inicio");
        inicio.ContentTemplate =
            new DataTemplate(() => services.GetRequiredService<InicioPage>());

        Routing.RegisterRoute(nameof(AsignaturasPage), typeof(AsignaturasPage));
        Routing.RegisterRoute(nameof(BancoPreguntasPage), typeof(BancoPreguntasPage));
        Routing.RegisterRoute(nameof(NuevaPreguntaPage), typeof(NuevaPreguntaPage));
        Routing.RegisterRoute(nameof(ConfigurarCuestionarioPage), typeof(ConfigurarCuestionarioPage));
        Routing.RegisterRoute(nameof(CuestionarioPage), typeof(CuestionarioPage));
    }
}