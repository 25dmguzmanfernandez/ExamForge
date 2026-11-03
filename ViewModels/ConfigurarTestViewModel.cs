using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;
using ExamForge.Helpers; // Asegúrate de apuntar a donde guardaste el CuestionarioEngine

namespace ExamForge.ViewModels
{
    public partial class ConfigurarTestViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private Asignatura _asignaturaActual;

        // Propiedades reactivas automáticas gracias al Toolkit
        [ObservableProperty]
        private string nombreAsignatura;

        [ObservableProperty]
        private int cantidadPreguntas = 10; // Valor inicial por defecto

        // Colección de temas adaptados para que la interfaz (XAML) pueda ponerles un Checkbox
        public ObservableCollection<TemaSeleccionableDto> TemasParaSeleccionar { get; set; } = new();

        public ConfigurarTestViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Se invoca al entrar a la pantalla pasando el ID de la asignatura seleccionada.
        /// </summary>
        public void Inicializar(string asignaturaId)
        {
            _asignaturaActual = _dataService.ObtenerUsuarios()
                .SelectMany(u => u.Cursos)
                .SelectMany(c => c.Asignaturas)
                .FirstOrDefault(a => a.Id == asignaturaId);

            if (_asignaturaActual == null) return;

            NombreAsignatura = _asignaturaActual.Nombre;
            TemasParaSeleccionar.Clear();

            // Mapeamos los temas originales a nuestro DTO con Checkbox
            foreach (var tema in _asignaturaActual.Temas)
            {
                TemasParaSeleccionar.Add(new TemaSeleccionableDto
                {
                    TemaRef = tema,
                    IsSelected = false
                });
            }
        }

        /// <summary>
        /// Comando asociado al botón "Comenzar Examen". Genera las preguntas y crea el cuestionario.
        /// </summary>
        [RelayCommand]
        private async Task IniciarCuestionario()
        {
            // 1. Filtrar los IDs de los temas que el usuario marcó en los Checkboxes
            var idsTemasSeleccionados = TemasParaSeleccionar
                .Where(t => t.IsSelected)
                .Select(t => t.TemaRef.Id)
                .ToList();

            // Validación: Al menos se debe seleccionar un tema
            if (!idsTemasSeleccionados.Any() || CantidadPreguntas <= 0)
            {
                // En MAUI puedes lanzar una alerta nativa así si lo deseas:
                // await Shell.Current.DisplayAlert("Atención", "Debes seleccionar al menos un tema y configurar preguntas.", "OK");
                return;
            }

            // 2. Invocar el motor de cuestionarios para obtener el bloque de preguntas equitativas/aleatorias
            var preguntasFiltradas = CuestionarioEngine.GenerarPreguntas(
                _asignaturaActual.Temas,
                idsTemasSeleccionados,
                CantidadPreguntas
            );

            // 3. Construir la entidad Cuestionario final
            var nuevoCuestionario = new Cuestionario
            {
                CantidadPreguntasSolicitadas = CantidadPreguntas,
                EsRepartoEquitativo = idsTemasSeleccionados.Count > 1, // Es equitativo automáticamente si selecciona más de 1 tema
                TemasIdsSeleccionados = idsTemasSeleccionados,
                Preguntas = preguntasFiltradas
            };

            // 4. Guardar en el histórico de la asignatura y persistir los cambios en el JSON del dispositivo
            _asignaturaActual.Cuestionarios.Add(nuevoCuestionario);
            _dataService.GuardarDatos();

            // 5. Navegación hacia la pantalla del Examen (CuestionarioPage) pasando el ID del test generado
            // Descomenta esto cuando configures tus rutas en AppShell.xaml.cs:
            // await Shell.Current.GoToAsync($"//CuestionarioPage?cuestionarioId={nuevoCuestionario.Id}");
        }
    }

    /// <summary>
    /// Clase de soporte (DTO) que envuelve el Tema original para añadirle la propiedad reactiva 'IsSelected'
    /// que se enlazará bidireccionalmente al Checkbox del XAML.
    /// </summary>
    public partial class TemaSeleccionableDto : ObservableObject
    {
        public Tema TemaRef { get; set; }

        [ObservableProperty]
        private bool isSelected;
    }
}
