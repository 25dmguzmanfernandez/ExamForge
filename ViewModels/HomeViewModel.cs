using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private Usuario? usuarioActual;
    [ObservableProperty] private int totalPreguntasRespondidasGlobal;
    [ObservableProperty] private int totalPreguntasAcertadasGlobal;
    [ObservableProperty] private double porcentajeAciertoGlobal;

    public ObservableCollection<Curso> Cursos { get; } = new();

    public HomeViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string usuarioId)
    {
        UsuarioActual = _dataService.UsuarioActivo;

        // Permite inicializar por id si todavía no se ha establecido la sesión activa.
        if (UsuarioActual is null || UsuarioActual.Id != usuarioId)
        {
            UsuarioActual = _dataService.ObtenerUsuarios()
                .FirstOrDefault(u => u.Id == usuarioId);
        }

        Recargar();
    }

    public void AgregarCurso(string nombre, string descripcion)
    {
        if (UsuarioActual is null) return;

        UsuarioActual.Cursos.Add(new Curso
        {
            Nombre = nombre.Trim(),
            Siglas = CrearSiglas(nombre),
            Descripcion = descripcion.Trim()
        });

        GuardarYRecargar();
    }

    public void EditarCurso(string id, string nombre, string descripcion)
    {
        var curso = UsuarioActual?.Cursos.FirstOrDefault(c => c.Id == id);
        if (curso is null) return;

        curso.Nombre = nombre.Trim();
        curso.Siglas = CrearSiglas(nombre);
        curso.Descripcion = descripcion.Trim();

        GuardarYRecargar();
    }

    public void EliminarCursos(IEnumerable<string> ids)
    {
        if (UsuarioActual is null) return;

        var idsEliminar = ids.ToHashSet();
        UsuarioActual.Cursos.RemoveAll(c => idsEliminar.Contains(c.Id));

        GuardarYRecargar();
    }

    public void Recargar()
    {
        Cursos.Clear();

        if (UsuarioActual is null)
        {
            TotalPreguntasRespondidasGlobal = 0;
            TotalPreguntasAcertadasGlobal = 0;
            PorcentajeAciertoGlobal = 0;
            return;
        }

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
        PorcentajeAciertoGlobal = respondidas > 0
            ? Math.Round((double)acertadas / respondidas * 100, 2)
            : 0;
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