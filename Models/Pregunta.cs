using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamForge.Models
{
    public class Pregunta
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Enunciado { get; set; } = "";
        public List<string> Opciones { get; set; } = new List<string>();
        public int RespuestaCorrecta { get; set; }
        public string FeedbackExplicacion { get; set; } = "";

        public bool EsValida()
        {
            if (string.IsNullOrWhiteSpace(Enunciado)) return false;
            if (Opciones == null || Opciones.Count < 2) return false;
            if (Opciones.Any(string.IsNullOrWhiteSpace)) return false;
            if (RespuestaCorrecta < 0 || RespuestaCorrecta >= Opciones.Count) return false;
            return true;
        }
    }
}
