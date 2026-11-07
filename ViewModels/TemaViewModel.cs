using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class TemaViewModel : ObservableObject
{
    private readonly DataService _dataService;
    private Tema? _temaActual;

    [ObservableProperty] private string nombreTema = "";

    public ObservableCollection<Pregunta> BancoDePreguntas { get; } = new();
    public ObservableCollection<Cuestionario> Cuestionarios { get; } = new();

    public Asignatura? AsignaturaActual { get; private set; }
    public Tema? TemaActual => _temaActual;

    public TemaViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string temaId)
    {
        AsignaturaActual = _dataService.UsuarioActivo?
    .Cursos
    .SelectMany(c => c.Asignaturas)
    .FirstOrDefault(a => a.Temas.Any(t => t.Id == temaId));

        _temaActual = AsignaturaActual?.Temas.FirstOrDefault(t => t.Id == temaId);

        var asignatura = _dataService.UsuarioActivo?
            .Cursos
            .SelectMany(c => c.Asignaturas)
            .FirstOrDefault(a => a.Temas.Any(t => t.Id == temaId));

        _temaActual = asignatura?.Temas.FirstOrDefault(t => t.Id == temaId);

        BancoDePreguntas.Clear();
        Cuestionarios.Clear();

        if (_temaActual is null || asignatura is null)
        {
            NombreTema = "";
            return;
        }

        NombreTema = _temaActual.Nombre;

        foreach (var pregunta in _temaActual.BancoDePreguntas)
            BancoDePreguntas.Add(pregunta);

        var idsPreguntas = _temaActual.BancoDePreguntas
            .Select(p => p.Id)
            .ToHashSet();

        foreach (var cuestionario in asignatura.Cuestionarios
                     .Where(c => c.Preguntas.Any(p => idsPreguntas.Contains(p.Id)))
                     .OrderByDescending(c => c.FechaCreacion))
        {
            Cuestionarios.Add(cuestionario);
        }
    }

    public void EliminarPreguntas(IEnumerable<string> ids)
    {
        if (_temaActual is null) return;

        var idsEliminar = ids.ToHashSet();
        _temaActual.BancoDePreguntas.RemoveAll(p => idsEliminar.Contains(p.Id));

        _dataService.GuardarDatos();
        Inicializar(_temaActual.Id);
    }
}