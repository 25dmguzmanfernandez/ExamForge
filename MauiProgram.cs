using Microsoft.Extensions.Logging;
using ExamForge.Services; // Asegúrate de importar el namespace de tu servicio

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

            // ViewModels Únicos o Globales
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();

            // ViewModels de Vistas de Detalle Dinámicas
            builder.Services.AddTransient<AsignaturaViewModel>();
            builder.Services.AddTransient<TemaViewModel>();
            builder.Services.AddTransient<ConfigurarTestViewModel>();
            builder.Services.AddTransient<CuestionarioViewModel>();



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
