using System;
using System.Collections.Generic;

namespace ExamForge.Models
{
    public class Curso
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = "";
        public string Siglas { get; set; } = "";

        public List<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();
    }
}
