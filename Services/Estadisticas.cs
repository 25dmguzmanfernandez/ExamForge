using ExamForge.Models;

namespace ExamForge.Services;

public static class Estadisticas
{
    public static (int Total, int SinContestar, int Correctas, int Incorrectas) Cifras(
        IEnumerable<Tema> temas,
        IEnumerable<Cuestionario> historico)
    {
        var ids = temas
            .SelectMany(t => t.BancoDePreguntas)
            .Select(p => p.Id)
            .ToHashSet();

        var ultimo = new Dictionary<string, bool>();

        foreach (var cuestionario in historico.OrderBy(c => c.FechaCreacion))
        {
            foreach (var pregunta in cuestionario.Preguntas)
            {
                if (ids.Contains(pregunta.Id) &&
                    cuestionario.RespuestasUsuario.TryGetValue(pregunta.Id, out int respuesta))
                {
                    ultimo[pregunta.Id] = respuesta == pregunta.RespuestaCorrecta;
                }
            }
        }

        int correctas = ultimo.Values.Count(acertada => acertada);

        return (
            Total: ids.Count,
            SinContestar: ids.Count - ultimo.Count,
            Correctas: correctas,
            Incorrectas: ultimo.Count - correctas
        );
    }
}