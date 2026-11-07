/*
 * VistasJerarquia.cs: envoltorios de Curso, Asignatura y Tema para las listas (CollectionView).
 * CursoVista / AsignaturaVista: datos de la card y QuesoDrawable con las cifras del gráfico.
 * TemaVista: un tema dentro del desplegable de una asignatura o como casilla en ConfigurarCuestionarioPage.
 */
using ExamForge.Models;
using ExamForge.Services;
using ExamForge.Views.Componentes;
using Microsoft.Maui.Graphics;

namespace ExamForge.Views.Vistas;

public class CursoVista : TarjetaVista
{
    public Curso Modelo { get; }
    public QuesoDrawable Queso { get; } = new();
    public int Correctas { get; }
    public int Incorrectas { get; }
    public int SinContestar { get; }
    public Color ColorCorrectas => Paleta.Verde;
    public Color ColorIncorrectas => Paleta.Rojo;
    public Color ColorSinContestar => Paleta.Gris;

    public CursoVista(Curso curso)
    {
        Modelo = curso;

        var c = Estadisticas.Cifras(
            curso.Asignaturas.SelectMany(a => a.Temas),
            curso.Asignaturas.SelectMany(a => a.Cuestionarios));

        Correctas = c.Correctas;
        Incorrectas = c.Incorrectas;
        SinContestar = c.SinContestar;
        Queso.Establecer(c.Total, c.SinContestar, c.Correctas, c.Incorrectas);
    }

    public string Siglas => Modelo.Siglas;
    public string Nombre => Modelo.Nombre;
    public string Descripcion => Modelo.Descripcion;
    public List<string> NombresAsignaturas => Modelo.Asignaturas.Select(a => a.Nombre).ToList();
    public bool SinAsignaturas => Modelo.Asignaturas.Count == 0;
}

public class AsignaturaVista : TarjetaVista
{
    public Asignatura Modelo { get; }
    public QuesoDrawable Queso { get; } = new();
    public int Correctas { get; }
    public int Incorrectas { get; }
    public int SinContestar { get; }
    public Color ColorCorrectas => Paleta.Verde;
    public Color ColorIncorrectas => Paleta.Rojo;
    public Color ColorSinContestar => Paleta.Gris;
    public List<TemaVista> Temas { get; }

    public AsignaturaVista(Asignatura asignatura)
    {
        Modelo = asignatura;
        Temas = asignatura.Temas.Select(t => new TemaVista(asignatura, t)).ToList();

        var c = Estadisticas.Cifras(asignatura.Temas, asignatura.Cuestionarios);

        Correctas = c.Correctas;
        Incorrectas = c.Incorrectas;
        SinContestar = c.SinContestar;
        Queso.Establecer(c.Total, c.SinContestar, c.Correctas, c.Incorrectas);
    }

    public string Siglas => Modelo.Siglas;
    public string Nombre => Modelo.Nombre;
    public string Descripcion => Modelo.Descripcion;
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

    public bool Seleccionado
    {
        get => seleccionado;
        set => Asignar(ref seleccionado, value);
    }
}