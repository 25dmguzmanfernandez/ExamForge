using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExamForge.Models;

namespace ExamForge.Services
{
    public static class DataService
    {
        // El usuario global de la aplicación (Mochila raíz)
        public static Usuario UsuarioActivo { get; set; } = new Usuario();

        /// <summary>
        /// Inicializa todos los datos de prueba simulando un estudiante real con historial.
        /// </summary>
        public static void InicializarMockData()
        {
            // 1. Crear el Perfil del Usuario
            UsuarioActivo.Nombre = "Santiago Barrera";
            UsuarioActivo.Email = "santiago@examforge.com";

            // 2. Crear un Curso y añadirlo al usuario
            var cursoDam = new Curso { Nombre = "1º Desarrollo de Aplicaciones Multiplataforma", Siglas = "1º DAM" };
            UsuarioActivo.Cursos.Add(cursoDam);

            // 3. Crear una Asignatura y añadirla al curso
            var asigProg = new Asignatura { Nombre = "Programación Estructurada y POO", Siglas = "PROG" };
            cursoDam.Asignaturas.Add(asigProg);

            // 4. Crear un Tema y añadirlo a la asignatura
            var temaPoo = new Tema { Nombre = "Programación Orientada a Objetos" };
            asigProg.Temas.Add(temaPoo);

            // 5. Inyectar preguntas de prueba con diferente número de opciones (2, 3 y 4)
            var p1 = new Pregunta
            {
                Enunciado = "¿Qué define una clase en C#?",
                Opciones = new List<string> { "La estructura y comportamiento de objetos", "Un tipo de bucle", "Una base de datos", "Un espacio de nombres" },
                RespuestaCorrecta = 0, // Opción A
                FeedbackExplicacion = "Una clase es la plantilla o molde que define las propiedades y métodos de un objeto."
            };

            var p2 = new Pregunta
            {
                Enunciado = "En C#, ¿una clase puede heredar directamente de múltiples clases base?",
                Opciones = new List<string> { "Sí, C# permite herencia múltiple de clases", "No, C# solo permite herencia simple de clases" },
                RespuestaCorrecta = 1, // Opción B (Verdadero/Falso)
                FeedbackExplicacion = "C# solo admite herencia simple de clases para evitar ambigüedades, aunque permite implementar múltiples interfaces."
            };

            var p3 = new Pregunta
            {
                Enunciado = "¿Qué palabra clave se usa para heredar una clase o implementar una interfaz en C#?",
                Opciones = new List<string> { "El símbolo de los dos puntos (:)", "La palabra 'extends'", "La palabra 'implements'" },
                RespuestaCorrecta = 0, // Opción A (3 opciones)
                FeedbackExplicacion = "A diferencia de Java, C# unifica la herencia y la implementación usando el operador de dos puntos (:)."
            };

            // Añadir las preguntas al banco del tema
            temaPoo.BancoDePreguntas.Add(p1);
            temaPoo.BancoDePreguntas.Add(p2);
            temaPoo.BancoDePreguntas.Add(p3);

            // 6. Simular un HISTORIAL: Creamos un cuestionario que el usuario ya ha respondido
            var examenPasado = new Cuestionario
            {
                CantidadPreguntasSolicitadas = 3,
                EsRepartoEquitativo = true,
                TemasIdsSeleccionados = new List<string> { temaPoo.Id }
            };

            // Metemos las preguntas en el cuestionario realizado
            examenPasado.Preguntas.Add(p1);
            examenPasado.Preguntas.Add(p2);
            examenPasado.Preguntas.Add(p3);

            // Simulamos las respuestas del usuario (Acertó P1 y P2, falló P3 eligiendo la opción 1)
            examenPasado.RespuestasUsuario.Add(p1.Id, 0); // Correcto (0 == 0)
            examenPasado.RespuestasUsuario.Add(p2.Id, 1); // Correcto (1 == 1)
            examenPasado.RespuestasUsuario.Add(p3.Id, 1); // Incorrecto (Respondió opción B, la correcta era A)

            // Guardamos este examen en el historial de la asignatura
            asigProg.Cuestionarios.Add(examenPasado);
        }
    }
}
