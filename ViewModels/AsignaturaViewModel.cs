using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels
{
    public partial class AsignaturaViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private Asignatura _asignatura;

        [ObservableProperty] private string nombreAsignatura;
        [ObservableProperty] private double rendimientoGlobal;
        public ObservableCollection<TemaDisplayDto> TemasConRendimiento { get; set; } = new();

        public AsignaturaViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        public void Inicializar(string asignaturaId)
        {
            _asignatura = _dataService.ObtenerUsuarios()
                .SelectMany(u => u.Cursos)
                .SelectMany(c => c.Asignaturas)
                .FirstOrDefault(a => a.Id == asignaturaId);

            if (_asignatura == null) return;

            NombreAsignatura = _asignatura.Nombre;
            RendimientoGlobal = _asignatura.PorcentajeAcierto;

            TemasConRendimiento.Clear();
            foreach (var tema in _asignatura.Temas)
            {
                TemasConRendimiento.Add(new TemaDisplayDto
                {
                    Id = tema.Id,
                    Nombre = tema.Nombre,
                    PorcentajeAcierto = tema.ObtenerPorcentaje(_asignatura.Cuestionarios),
                    TotalPreguntasBanco = tema.BancoDePreguntas.Count
                });
            }
        }
    }

    // Objeto ligero para mapear los cálculos dinámicos de los temas en la UI
    public class TemaDisplayDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public double PorcentajeAcierto { get; set; }
        public int TotalPreguntasBanco { get; set; }
    }
}
