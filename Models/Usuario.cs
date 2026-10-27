using System;
using System.Collections.Generic;

namespace ExamForge.Models
{
    public class Usuario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";

        public List<Curso> Cursos { get; set; } = new List<Curso>();
    }
}
