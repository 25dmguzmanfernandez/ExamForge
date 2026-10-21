using System;
using System.Collections.Generic;
using System.Text;

namespace ExamenForge.Models
{
    public class Pregunta
    {
        public int Id { get; set; }

        public string Enunciado { get; set; } = "";

        public string OpcionA { get; set; } = "";
        public string OpcionB { get; set; } = "";
        public string OpcionC { get; set; } = "";
        public string OpcionD { get; set; } = "";

        public int RespuestaCorrecta { get; set; }

        public string Tema { get; set; } = "";
    }
}

