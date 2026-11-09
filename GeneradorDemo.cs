
using ExamForge.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ExamForge.Services
{
    public static class GeneradorDemo
    {
        private static readonly Random random = new Random(42);

        private static readonly string[][] materias =
        {
            new[] { "Programación", "Variables", "Condicionales", "Bucles" },
            new[] { "Programación orientada a objetos", "Clases", "Herencia", "Polimorfismo" },
            new[] { "Bases de datos", "Tablas", "Consultas SQL", "Transacciones" },
            new[] { "Sistemas operativos", "Procesos", "Memoria RAM", "Permisos" },
            new[] { "Redes", "Direcciones IP", "TCP y UDP", "DNS" },
            new[] { "Seguridad informática", "Autenticación", "Malware", "Cifrado" },
            new[] { "Desarrollo web", "HTML", "CSS", "JavaScript" },
            new[] { "Ingeniería del software", "Git", "Pruebas", "Patrones de diseño" },
            new[] { "Desarrollo móvil", "Ciclo de vida", "Persistencia", "Interfaces" },
            new[] { "Entornos de desarrollo", "Compiladores", "Depuración", "Integración continua" },
            new[] { "Inteligencia artificial", "Datos", "Entrenamiento", "Evaluación" },
            new[] { "Gestión de proyectos", "Planificación", "Scrum", "Kanban" }
        };

        private static readonly string[] definiciones =
        {
            "Almacenan valores que pueden cambiar",
            "Permiten tomar decisiones según una condición",
            "Repiten instrucciones bajo determinadas condiciones",
            "Definen la estructura y comportamiento de los objetos",
            "Permite reutilizar miembros de otras clases",
            "Permite distintas implementaciones de una operación",
            "Organizan información en filas y columnas",
            "Permiten recuperar y modificar datos",
            "Agrupan operaciones que deben ejecutarse consistentemente",
            "Son programas que se encuentran en ejecución",
            "Almacena temporalmente datos utilizados por el sistema",
            "Controlan el acceso a archivos y recursos",
            "Identifican interfaces dentro de una red",
            "Son protocolos de transporte de información",
            "Traduce nombres de dominio a direcciones IP",
            "Comprueba la identidad de un usuario",
            "Es software diseñado para realizar acciones maliciosas",
            "Protege información mediante técnicas criptográficas",
            "Define la estructura del contenido web",
            "Establece los estilos visuales de una página",
            "Es un lenguaje de programación utilizado en la web",
            "Permite controlar versiones del código fuente",
            "Comprueban el comportamiento del software",
            "Ofrecen soluciones reutilizables de diseño",
            "Gestiona los distintos estados de una aplicación",
            "Conserva información entre ejecuciones",
            "Permiten la interacción entre usuarios y aplicaciones",
            "Transforman código fuente en otra representación",
            "Ayuda a detectar errores durante la ejecución",
            "Automatiza la integración y validación de cambios",
            "Es información utilizada para el análisis",
            "Ajusta los parámetros de un modelo mediante ejemplos",
            "Mide el rendimiento de los modelos",
            "Organiza tareas, recursos y plazos",
            "Es un marco ágil que trabaja mediante iteraciones",
            "Permite visualizar y gestionar el flujo de trabajo"
        };

        private static Pregunta CrearPregunta(
            string nombreTema, int indiceTema, int numero)
        {
            string[] enunciados =
            {
                $"¿Qué describe mejor {nombreTema}?",
                $"Selecciona la definición de {nombreTema}.",
                $"¿Qué significa {nombreTema}?",
                $"¿Qué característica tiene {nombreTema}?",
                $"¿Qué explicación corresponde a {nombreTema}?",
                $"¿Cuál es la finalidad de {nombreTema}?",
                $"¿Qué afirmación es correcta sobre {nombreTema}?",
                $"Identifica el significado de {nombreTema}.",
                $"¿Qué opción se relaciona con {nombreTema}?",
                $"Selecciona la descripción correcta de {nombreTema}."
            };

            string correcta = definiciones[indiceTema];

            List<string> opciones = new List<string> { correcta };

            while (opciones.Count < 4)
            {
                int indice = random.Next(definiciones.Length);
                string opcion = definiciones[indice];

                if (!opciones.Contains(opcion))
                    opciones.Add(opcion);
            }

            opciones = opciones.OrderBy(x => random.Next()).ToList();

            return new Pregunta
            {
                Enunciado = enunciados[numero],
                Opciones = opciones,
                RespuestaCorrecta = opciones.IndexOf(correcta),
                FeedbackExplicacion = "La respuesta correcta es: " + correcta
            };
        }

        private static Cuestionario CrearCuestionario(
            Asignatura asignatura, int numero)
        {
            List<Pregunta> banco = asignatura.Temas
                .SelectMany(t => t.BancoDePreguntas)
                .ToList();

            List<Pregunta> preguntas = banco
                .OrderBy(x => random.Next())
                .Take(15)
                .ToList();

            Cuestionario cuestionario = new Cuestionario
            {
                FechaCreacion = DateTime.Now.AddDays(-40 + numero * 12),
                CantidadPreguntasSolicitadas = 15,
                EsRepartoEquitativo = true,
                TemasIdsSeleccionados = asignatura.Temas
                    .Select(t => t.Id)
                    .ToList(),
                Preguntas = preguntas
            };

            double probabilidad = 0.45 + numero * 0.20;

            foreach (Pregunta pregunta in preguntas)
            {
                bool acierto = random.NextDouble() < probabilidad;

                int respuesta = acierto
                    ? pregunta.RespuestaCorrecta
                    : (pregunta.RespuestaCorrecta + 1) % pregunta.Opciones.Count;

                cuestionario.RespuestasUsuario[pregunta.Id] = respuesta;
            }

            return cuestionario;
        }

        public static string Crear(DataService datos)
        {

            Usuario? usuarioExistente = datos.ObtenerUsuarios()
            .FirstOrDefault(u => u.Nombre == "DemoExamForge");

            if (usuarioExistente != null)
            {
                return "El usuario ya existe. No se generan datos.";
            }

            if (datos == null)
                return "No se ha recibido DataService.";

            if (datos.ObtenerUsuarios().Any(u =>
                u.Nombre.Equals("DemoExamForge",
                    StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals("demo.examforge@example.com",
                    StringComparison.OrdinalIgnoreCase)))
            {
                return "El usuario de demostración ya existe. No se ha modificado nada.";
            }

            // Copia de seguridad del JSON antes de modificarlo
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "data.json");

            if (File.Exists(ruta))
            {
                string copia = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "data.backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".json"
                );

                File.Copy(ruta, copia);
            }

            Usuario usuario = new Usuario
            {
                Nombre = "DemoExamForge",
                Apellidos = "Presentacion",
                Email = "demo.examforge@example.com",
                Password = PasswordHasher.Crear("Demo2026!")
            };

            string[] nombresCursos =
            {
                "1.º Desarrollo de Software",
                "2.º Sistemas y Redes",
                "3.º Aplicaciones y Calidad",
                "4.º Datos y Gestión"
            };

            int indiceTema = 0;

            for (int c = 0; c < 4; c++)
            {
                Curso curso = new Curso
                {
                    Nombre = nombresCursos[c],
                    Siglas = "CUR" + (c + 1),
                    Descripcion = "Curso de demostración de ExamForge"
                };

                for (int a = 0; a < 3; a++)
                {
                    int indiceAsignatura = c * 3 + a;
                    string[] materia = materias[indiceAsignatura];

                    Asignatura asignatura = new Asignatura
                    {
                        Nombre = materia[0],
                        Siglas = "ASG" + (indiceAsignatura + 1),
                        Descripcion = "Asignatura para probar ExamForge"
                    };

                    for (int t = 1; t <= 3; t++)
                    {
                        Tema tema = new Tema
                        {
                            Nombre = materia[t],
                            Descripcion = "Tema de demostración: " + materia[t]
                        };

                        for (int p = 0; p < 10; p++)
                        {
                            Pregunta pregunta = CrearPregunta(
                                tema.Nombre, indiceTema, p
                            );

                            tema.BancoDePreguntas.Add(pregunta);
                        }

                        asignatura.Temas.Add(tema);
                        indiceTema++;
                    }

                    for (int q = 0; q < 3; q++)
                    {
                        Cuestionario cuestionario = CrearCuestionario(
                            asignatura, q
                        );

                        asignatura.Cuestionarios.Add(cuestionario);
                    }

                    curso.Asignaturas.Add(asignatura);
                }

                usuario.Cursos.Add(curso);
            }

            datos.ObtenerUsuarios().Add(usuario);
            datos.GuardarDatos();

            // Comprobación del archivo persistente
            if (!File.Exists(ruta))
                return "No se ha encontrado el archivo de persistencia.";

            string contenido = File.ReadAllText(ruta);

            if (!contenido.Contains("demo.examforge@example.com"))
                return "No se ha podido confirmar el guardado de la demo.";

            return "Demo creada y guardada correctamente: 4 cursos, 12 asignaturas, 36 temas, 360 preguntas y 36 cuestionarios.";
        }
    }
}
