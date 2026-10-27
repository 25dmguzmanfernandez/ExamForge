using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamForge.Models
{
    public class Cuestionario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Configuración del test
        public int CantidadPreguntasSolicitadas { get; set; }
        public bool EsRepartoEquitativo { get; set; } = true;

        // IDs de los temas de la asignatura que entran en este cuestionario
        public List<string> TemasIdsSeleccionados { get; set; } = new List<string>();

        // Preguntas seleccionadas finalmente por el algoritmo de Samuel
        public List<Pregunta> Preguntas { get; set; } = new List<Pregunta>();

        // Respuestas guardadas (Key: PreguntaId, Value: Índice de opción seleccionada)
        public Dictionary<string, int> RespuestasUsuario { get; set; } = new Dictionary<string, int>();

        // Propiedades calculadas automáticas
        public int TotalPreguntas => Preguntas.Count;
        public int Aciertos => Preguntas.Count(p => RespuestasUsuario.ContainsKey(p.Id) && RespuestasUsuario[p.Id] == p.RespuestaCorrecta);
        public double PorcentajeAcierto => TotalPreguntas > 0 ? ((double)Aciertos / TotalPreguntas) * 100 : 0;
    }
}
