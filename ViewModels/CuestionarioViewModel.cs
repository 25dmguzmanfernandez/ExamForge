using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels
{
    public partial class CuestionarioViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private Cuestionario cuestionarioActual;

        [ObservableProperty]
        private int indicePreguntaActual = 0;

        [ObservableProperty]
        private Pregunta preguntaActual;

        [ObservableProperty]
        private string textoProgreso; // Ej: "Pregunta 3 de 10"

        public CuestionarioViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        // Se ejecuta al abrir la pantalla pasando el ID del test que generó la pantalla anterior
        public void Inicializar(string cuestionarioId)
        {
            CuestionarioActual = _dataService.ObtenerUsuarios()
                .SelectMany(u => u.Cursos)
                .SelectMany(c => c.Asignaturas)
                .SelectMany(a => a.Cuestionarios)
                .FirstOrDefault(c => c.Id == cuestionarioId);

            if (CuestionarioActual != null && CuestionarioActual.Preguntas.Any())
            {
                IndicePreguntaActual = 0;
                ActualizarEstadoPantalla();
            }
        }

        private void ActualizarEstadoPantalla()
        {
            PreguntaActual = CuestionarioActual.Preguntas[IndicePreguntaActual];
            TextoProgreso = $"Pregunta {IndicePreguntaActual + 1} de {CuestionarioActual.TotalPreguntas}";
        }

        // Este comando se enlaza al botón "Siguiente" del XAML
        [RelayCommand]
        private void SiguientePregunta()
        {
            if (IndicePreguntaActual < CuestionarioActual.TotalPreguntas - 1)
            {
                IndicePreguntaActual++;
                ActualizarEstadoPantalla();
            }
        }

        // Este comando guarda la respuesta del usuario cuando pulsa una opción en la pantalla
        [RelayCommand]
        private void RegistrarRespuesta(int indiceOpcionSeleccionada)
        {
            if (PreguntaActual == null) return;

            // Guardamos la respuesta en el diccionario del cuestionario
            CuestionarioActual.RespuestasUsuario[PreguntaActual.Id] = indiceOpcionSeleccionada;

            // Sincronizamos los datos en tu archivo JSON local
            _dataService.GuardarDatos();

            // Avisamos a la pantalla de que el cuestionario se actualizó (recalcula aciertos en la UI)
            OnPropertyChanged(nameof(CuestionarioActual));
        }
    }
}
