using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamForge.Models
{
    public class Asignatura : IEstadisticas
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = "";
        public string Siglas { get; set; } = "";

        public List<Tema> Temas { get; set; } = new List<Tema>();
        public List<Cuestionario> Cuestionarios { get; set; } = new List<Cuestionario>();

        public int TotalPreguntasRespondidas => Cuestionarios.Sum(c => c.TotalPreguntas);

        public int TotalPreguntasAcertadas => Cuestionarios.Sum(c => c.Aciertos);

        public double PorcentajeAcierto => TotalPreguntasRespondidas > 0
            ? Math.Round(((double)TotalPreguntasAcertadas / TotalPreguntasRespondidas) * 100, 2)
            : 0;
    }
}
