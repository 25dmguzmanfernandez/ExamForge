using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamForge.Models
{
    public class Tema
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = "";
        public List<Pregunta> BancoDePreguntas { get; set; } = new List<Pregunta>();

        // Métodos de cálculo dinámico (reciben el histórico de la asignatura)
        public int ObtenerRespondidas(List<Cuestionario> historico) =>
            historico.Sum(c => c.Preguntas.Count(p => BancoDePreguntas.Any(b => b.Id == p.Id)));

        public int ObtenerAcertadas(List<Cuestionario> historico)
        {
            int aciertos = 0;
            foreach (var c in historico)
            {
                var preguntasTema = c.Preguntas.Where(p => BancoDePreguntas.Any(b => b.Id == p.Id));
                aciertos += preguntasTema.Count(p => c.RespuestasUsuario.TryGetValue(p.Id, out int r) && r == p.RespuestaCorrecta);
            }
            return aciertos;
        }

        public double ObtenerPorcentaje(List<Cuestionario> historico)
        {
            int resp = ObtenerRespondidas(historico);
            return resp > 0 ? Math.Round(((double)ObtenerAcertadas(historico) / resp) * 100, 2) : 0;
        }
    }
}
