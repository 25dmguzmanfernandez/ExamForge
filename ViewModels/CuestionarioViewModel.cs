using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class CuestionarioViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private Cuestionario? cuestionarioActual;
    [ObservableProperty] private bool esRevision;

    public CuestionarioViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string? examenId = null)
    {
        EsRevision = !string.IsNullOrWhiteSpace(examenId);

        if (EsRevision)
        {
            CuestionarioActual = _dataService.UsuarioActivo?
                .Cursos
                .SelectMany(c => c.Asignaturas)
                .SelectMany(a => a.Cuestionarios)
                .FirstOrDefault(c => c.Id == examenId);

            return;
        }

        CuestionarioActual = _dataService.CuestionarioEnCurso;
    }

    public double Finalizar(Dictionary<string, int> respuestas)
    {
        if (CuestionarioActual is null || _dataService.AsignaturaEnCurso is null)
            return 0;

        var preguntasPorId = CuestionarioActual.Preguntas
            .ToDictionary(p => p.Id);

        foreach (var respuesta in respuestas)
        {
            if (preguntasPorId.TryGetValue(respuesta.Key, out var pregunta) &&
                respuesta.Value >= 0 &&
                respuesta.Value < pregunta.Opciones.Count)
            {
                CuestionarioActual.RespuestasUsuario[respuesta.Key] = respuesta.Value;
            }
        }

        _dataService.AsignaturaEnCurso.Cuestionarios.Add(CuestionarioActual);
        _dataService.GuardarDatos();

        _dataService.CuestionarioEnCurso = null;
        _dataService.AsignaturaEnCurso = null;

        return Math.Round(CuestionarioActual.PorcentajeAcierto / 10, 1);
    }

    public void CancelarIntento()
    {
        _dataService.CuestionarioEnCurso = null;
        _dataService.AsignaturaEnCurso = null;
    }
}