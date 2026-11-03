using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty] private Usuario usuarioActual;
        [ObservableProperty] private int totalPreguntasRespondidasGlobal;
        [ObservableProperty] private int totalPreguntasAcertadasGlobal;
        [ObservableProperty] private double porcentajeAciertoGlobal;

        public ObservableCollection<Curso> Cursos { get; set; } = new();

        public HomeViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        public void Inicializar(string usuarioId)
        {
            UsuarioActual = _dataService.ObtenerUsuarios().FirstOrDefault(u => u.Id == usuarioId);
            if (UsuarioActual == null) return;

            Cursos.Clear();
            int respondidas = 0;
            int acertadas = 0;

            foreach (var curso in UsuarioActual.Cursos)
            {
                Cursos.Add(curso);
                foreach (var asignatura in curso.Asignaturas)
                {
                    respondidas += asignatura.TotalPreguntasRespondidas;
                    acertadas += asignatura.TotalPreguntasAcertadas;
                }
            }

            TotalPreguntasRespondidasGlobal = respondidas;
            TotalPreguntasAcertadasGlobal = acertadas;
            PorcentajeAciertoGlobal = respondidas > 0 ? Math.Round(((double)acertadas / respondidas) * 100, 2) : 0;
        }
    }
}
