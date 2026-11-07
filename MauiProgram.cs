using ExamForge.Services;
using ExamForge.ViewModels;
using ExamForge.Views;
using Microsoft.Extensions.Logging;

namespace ExamForge
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //Todos los servicios y ViewModels se registran usando 'builder.Services'
            builder.Services.AddSingleton<DataService>();

            // ViewModels Únicos o Globales
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();

            // ViewModels de Vistas de Detalle Dinámicas
            builder.Services.AddTransient<AsignaturaViewModel>();
            builder.Services.AddTransient<TemaViewModel>();
            builder.Services.AddTransient<ConfigurarTestViewModel>();
            builder.Services.AddTransient<CuestionarioViewModel>();

            // y el resto de ViewModels
            builder.Services.AddTransient<PortalPage>();
            builder.Services.AddTransient<InicioPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
