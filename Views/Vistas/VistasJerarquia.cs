/*
 * VistasJerarquia.cs: envoltorios de Curso, Asignatura y Tema para las listas (CollectionView).
 * CursoVista / AsignaturaVista: datos de la card + QuesoDrawable con las cifras del gráfico.
 * TemaVista: un tema dentro del desplegable de una asignatura o como casilla en ConfigurarCuestionarioPage.
 * Actualizar() recalcula las cifras llamando a Provisional.Cifras (TODO modelo/controlador).
 */
using ExamForge.Models;
using Microsoft.Maui.Graphics;
using ExamForge.Views.Componentes;

namespace ExamForge.Views.Vistas;

public class CursoVista : TarjetaVista
{
    public Curso Modelo { get; }
    public QuesoDrawable Queso { get; } = new();
    public string ResumenQueso { get; private set; } = "";

    public CursoVista(Curso curso)
    {
        Modelo = curso;
        var c = Provisional.Cifras(curso.Asignaturas.SelectMany(a => a.Temas),
                                   curso.Asignaturas.SelectMany(a => a.Cuestionarios));
        Queso.Establecer(c.Total, c.SinContestar, c.Correctas, c.Incorrectas);
        ResumenQueso = $"✔ {c.Correctas}  ✖ {c.Incorrectas}  ○ {c.SinContestar}";
    }

    public string Siglas => Modelo.Siglas;
    public string Nombre => Modelo.Nombre;
    public string Descripcion => Provisional.Descripcion(Modelo.Id);
    public List<string> NombresAsignaturas => Modelo.Asignaturas.Select(a => a.Nombre).ToList();
    public bool SinAsignaturas => Modelo.Asignaturas.Count == 0;
}

public class AsignaturaVista : TarjetaVista
{
    public Asignatura Modelo { get; }
    public QuesoDrawable Queso { get; } = new();
    public string ResumenQueso { get; private set; } = "";
    public List<TemaVista> Temas { get; }

    public AsignaturaVista(Asignatura asignatura)
    {
        Modelo = asignatura;
        Temas = asignatura.Temas.Select(t => new TemaVista(asignatura, t)).ToList();
        var c = Provisional.Cifras(asignatura.Temas, asignatura.Cuestionarios);
        Queso.Establecer(c.Total, c.SinContestar, c.Correctas, c.Incorrectas);
        ResumenQueso = $"✔ {c.Correctas}  ✖ {c.Incorrectas}  ○ {c.SinContestar}";
    }

    public string Siglas => Modelo.Siglas;
    public string Nombre => Modelo.Nombre;
    public string Descripcion => Provisional.Descripcion(Modelo.Id);
    public bool SinTemas => Temas.Count == 0;
}

public class TemaVista : ObjetoVista
{
    private bool seleccionado;

    public Asignatura Asignatura { get; }
    public Tema Modelo { get; }

    public TemaVista(Asignatura asignatura, Tema tema)
    {
        Asignatura = asignatura;
        Modelo = tema;
    }

    public string Nombre => Modelo.Nombre;
    public int TotalPreguntas => Modelo.BancoDePreguntas.Count;
    public string Texto => $"{Nombre} ({TotalPreguntas})";
    public bool Seleccionado { get => seleccionado; set => Asignar(ref seleccionado, value); }
}
