using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class AsignaturasViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private Curso? cursoActual;

    public ObservableCollection<Asignatura> Asignaturas { get; } = new();

    public AsignaturasViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string cursoId)
    {
        CursoActual = _dataService.UsuarioActivo?
            .Cursos.FirstOrDefault(c => c.Id == cursoId);

        Recargar();
    }

    public void AgregarAsignatura(string nombre, string descripcion)
    {
        if (CursoActual is null) return;

        CursoActual.Asignaturas.Add(new Asignatura
        {
            Nombre = nombre.Trim(),
            Siglas = CrearSiglas(nombre),
            Descripcion = descripcion.Trim()
        });

        GuardarYRecargar();
    }

    public void EditarAsignatura(string id, string nombre, string descripcion)
    {
        var asignatura = CursoActual?.Asignaturas.FirstOrDefault(a => a.Id == id);
        if (asignatura is null) return;

        asignatura.Nombre = nombre.Trim();
        asignatura.Siglas = CrearSiglas(nombre);
        asignatura.Descripcion = descripcion.Trim();

        GuardarYRecargar();
    }

    public void EliminarAsignaturas(IEnumerable<string> ids)
    {
        if (CursoActual is null) return;

        var idsEliminar = ids.ToHashSet();
        CursoActual.Asignaturas.RemoveAll(a => idsEliminar.Contains(a.Id));

        GuardarYRecargar();
    }

    public void AgregarTema(string asignaturaId, string nombre)
    {
        var asignatura = CursoActual?.Asignaturas.FirstOrDefault(a => a.Id == asignaturaId);
        if (asignatura is null) return;

        asignatura.Temas.Add(new Tema { Nombre = nombre.Trim() });

        GuardarYRecargar();
    }

    public void Recargar()
    {
        Asignaturas.Clear();

        if (CursoActual is null) return;

        foreach (var asignatura in CursoActual.Asignaturas)
            Asignaturas.Add(asignatura);
    }

    private void GuardarYRecargar()
    {
        _dataService.GuardarDatos();
        Recargar();
    }

    private static string CrearSiglas(string nombre)
    {
        var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length > 2)
            .ToList();

        if (palabras.Count == 0)
            return nombre.Trim().ToUpperInvariant();

        if (palabras.Count == 1)
            return palabras[0][..Math.Min(4, palabras[0].Length)].ToUpperInvariant();

        return string.Concat(palabras.Take(5).Select(p => char.ToUpperInvariant(p[0])));
    }
}