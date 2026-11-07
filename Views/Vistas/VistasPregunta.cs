/*
 * VistasPregunta.cs: envoltorios de preguntas y exámenes para banco, cuestionario y revisión.
 * OpcionVista: una respuesta (letra, texto, colores) y si está marcada (RadioButton).
 * PreguntaVista: una pregunta con sus opciones. Fábricas: ParaBanco, ParaCuestionario y ParaRevision.
 *   Las opciones se muestran en el orden que entrega la lógica (ya barajadas); RespuestaCorrecta es el índice mostrado.
 * ExamenVista: una fila del historial: título, fecha y puntuación N/total (verde si correctas > total/2, rojo si no).
 */
using ExamForge.Models;
using Microsoft.Maui.Graphics;
using ExamForge.Views.Componentes;

namespace ExamForge.Views.Vistas;

public class OpcionVista : ObjetoVista
{
    private bool seleccionada;

    public string Grupo { get; init; } = "";
    public string Texto { get; init; } = "";
    public Color Fondo { get; init; } = Colors.Transparent;
    public Color ColorTexto { get; init; } = Paleta.Texto;
    public bool SoloLectura { get; init; }
    public bool Seleccionada { get => seleccionada; set => Asignar(ref seleccionada, value); }
}

public class PreguntaVista : TarjetaVista
{
    public Pregunta Modelo { get; }
    public string TituloNumerado { get; private set; } = "";
    public string Aviso { get; private set; } = "";
    public List<OpcionVista> Opciones { get; private set; } = new();

    private PreguntaVista(Pregunta pregunta) => Modelo = pregunta;

    public string Enunciado => Modelo.Enunciado;

    private static string Letra(int i) => ((char)('A' + i)).ToString();

    public static PreguntaVista ParaBanco(Pregunta p)
    {
        var vista = new PreguntaVista(p) { TituloNumerado = p.Enunciado };
        vista.Opciones = p.Opciones.Select((texto, i) => new OpcionVista
        {
            Texto = $"{Letra(i)}) {texto}" + (i == p.RespuestaCorrecta ? "  ✔" : "")
        }).ToList();
        return vista;
    }

    public static PreguntaVista ParaCuestionario(Pregunta p, int numero)
    {
        var vista = new PreguntaVista(p) { TituloNumerado = $"{numero}. {p.Enunciado}" };
        vista.Opciones = p.Opciones.Select((texto, i) => new OpcionVista
        {
            Grupo = p.Id,
            Texto = $"{Letra(i)}) {texto}"
        }).ToList();
        return vista;
    }

    public static PreguntaVista ParaRevision(Pregunta p, Cuestionario c, int numero)
    {
        int elegida = c.RespuestasUsuario.TryGetValue(p.Id, out int r) ? r : -1;
        var vista = new PreguntaVista(p)
        {
            TituloNumerado = $"{numero}. {p.Enunciado}",
            Aviso = elegida == -1 ? "Sin responder" : ""
        };

        vista.Opciones = p.Opciones.Select((texto, i) =>
        {
            bool correcta = i == p.RespuestaCorrecta;
            bool marcada = i == elegida;
            return new OpcionVista
            {
                Grupo = p.Id,
                Texto = $"{Letra(i)}) {texto}" + (correcta ? "  ✔" : marcada ? "  ✖ tu respuesta" : ""),
                Fondo = correcta ? Paleta.Verde : marcada ? Paleta.Rojo : Colors.Transparent,
                ColorTexto = correcta || marcada ? Colors.White : Paleta.Texto,
                SoloLectura = true,
                Seleccionada = marcada
            };
        }).ToList();
        return vista;
    }
}

public class ExamenVista
{
    public Cuestionario Modelo { get; }
    public string Titulo { get; }
    public string Fecha { get; }
    public string Puntuacion { get; }
    public Color ColorPuntuacion { get; }

    /// <param name="tema">Si se indica, la puntuación cuenta solo las preguntas de ese tema.</param>
    public ExamenVista(Cuestionario cuestionario, int numero, Tema? tema)
    {
        Modelo = cuestionario;

        int total, aciertos;
        if (tema != null)
        {
            var solo = new List<Cuestionario> { cuestionario };
            total = tema.ObtenerRespondidas(solo);
            aciertos = tema.ObtenerAcertadas(solo);
        }
        else
        {
            total = cuestionario.TotalPreguntas;
            aciertos = cuestionario.Aciertos;
        }

        Titulo = $"Examen Nº {numero}";
        Fecha = cuestionario.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
        Puntuacion = $"{aciertos} / {total}";
        ColorPuntuacion = aciertos > total / 2.0 ? Paleta.Verde : Paleta.Rojo;
    }
}
