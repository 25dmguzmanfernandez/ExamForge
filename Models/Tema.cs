using System;
using System.Collections.Generic;
using System.Text;

namespace ExamenForge.Models
{
    public class Tema
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public int AsignaturaId { get; set; }
    }
}
