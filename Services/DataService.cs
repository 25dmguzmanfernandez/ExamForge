using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExamForge.Models;

namespace ExamForge.Services
{
    /// <summary>
    /// Lógica de acceso a los datos de la aplicación.
    /// Los controladores pueden usar estos métodos sin acceder al JSON.
    /// </summary>
    public static class DataService
    {
        // La estructura del repositorio es Usuario -> Curso -> Asignatura -> Tema -> Pregunta.
        public static Usuario UsuarioActivo { get; set; } = new Usuario();

        private static bool cargado = false;
        private static readonly SemaphoreSlim bloqueo = new(1, 1);

        // Esta función se llama teniendo adquirido "bloqueo".
        private static async Task CargarInternamenteAsync()
        {
            if (cargado) return;

            Usuario? usuarioGuardado = await DataStorage.CargarAsync();
            if (usuarioGuardado != null)
                UsuarioActivo = usuarioGuardado;

            // Solo marcar como cargado al terminar correctamente.
            cargado = true;
        }

        /// <summary>Leer el JSON una sola vez al entrar en la aplicación.</summary>
        public static async Task CargarAsync()
        {
            await bloqueo.WaitAsync();
            try
            {
                await CargarInternamenteAsync();
            }
            finally
            {
                bloqueo.Release();
            }
        }

        /// <summary>
        /// Guardar cambios hechos directamente sobre los modelos (por ejemplo,
        /// las respuestas de un cuestionario). Conviene llamar antes a CargarAsync().
        /// </summary>
        public static async Task GuardarAsync()
        {
            await bloqueo.WaitAsync();
            try
            {
                await CargarInternamenteAsync();
                await DataStorage.GuardarAsync(UsuarioActivo);
            }
            finally
            {
                bloqueo.Release();
            }
        }

        // Operación común para crear, editar o eliminar: cargar -> cambiar -> guardar.
        // El bloqueo evita que dos acciones a la vez escriban simultáneamente el archivo.
        private static async Task<T> CambiarYGuardarAsync<T>(Func<T> cambio)
        {
            await bloqueo.WaitAsync();
            try
            {
                await CargarInternamenteAsync();
                T resultado = cambio();
                await DataStorage.GuardarAsync(UsuarioActivo);
                return resultado;
            }
            finally
            {
                bloqueo.Release();
            }
        }

        // --- CONSULTAS (primero llamar a CargarAsync una vez) ---

        public static List<Curso> ObtenerCursos() => UsuarioActivo.Cursos.ToList();

        public static List<Asignatura> ObtenerAsignaturas() =>
            UsuarioActivo.Cursos.SelectMany(c => c.Asignaturas).ToList();

        public static List<Tema> ObtenerTemas() =>
            ObtenerAsignaturas().SelectMany(a => a.Temas).ToList();

        public static List<Pregunta> ObtenerPreguntas() =>
            ObtenerTemas().SelectMany(t => t.BancoDePreguntas).ToList();

        public static List<Pregunta> ObtenerPreguntasDeTema(string temaId) =>
            BuscarTema(temaId)?.BancoDePreguntas.ToList() ?? new List<Pregunta>();

        public static Curso? BuscarCurso(string id) =>
            UsuarioActivo.Cursos.FirstOrDefault(c => c.Id == id);

        public static Asignatura? BuscarAsignatura(string id) =>
            ObtenerAsignaturas().FirstOrDefault(a => a.Id == id);

        public static Tema? BuscarTema(string id) =>
            ObtenerTemas().FirstOrDefault(t => t.Id == id);

        public static Pregunta? BuscarPregunta(string id) =>
            ObtenerPreguntas().FirstOrDefault(p => p.Id == id);

        public static Tema? BuscarTemaDePregunta(string preguntaId) =>
            ObtenerTemas().FirstOrDefault(t =>
                t.BancoDePreguntas.Any(p => p.Id == preguntaId));

        private static string NombreObligatorio(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            return nombre.Trim();
        }

        // --- CURSOS ---

        public static Task<Curso> CrearCursoAsync(string nombre, string siglas = "") =>
            CambiarYGuardarAsync(() =>
            {
                var curso = new Curso
                {
                    Nombre = NombreObligatorio(nombre),
                    Siglas = siglas?.Trim() ?? ""
                };
                UsuarioActivo.Cursos.Add(curso);
                return curso;
            });

        public static Task<Curso> EditarCursoAsync(string id, string nombre, string siglas = "") =>
            CambiarYGuardarAsync(() =>
            {
                Curso curso = BuscarCurso(id) ?? throw new KeyNotFoundException("Curso no encontrado.");
                curso.Nombre = NombreObligatorio(nombre);
                curso.Siglas = siglas?.Trim() ?? "";
                return curso;
            });

        public static Task<bool> EliminarCursoAsync(string id) =>
            CambiarYGuardarAsync(() =>
            {
                Curso? curso = BuscarCurso(id);
                return curso != null && UsuarioActivo.Cursos.Remove(curso);
            });

        // --- ASIGNATURAS ---

        /// <summary>
        /// Si se omite cursoId, se usa el primer curso o se crea "Mi curso".
        /// Esta opción encaja con la interfaz actual, que no permite elegir curso.
        /// </summary>
        public static Task<Asignatura> CrearAsignaturaAsync(
            string nombre, string siglas = "", string? cursoId = null) =>
            CambiarYGuardarAsync(() =>
            {
                string nombreLimpio = NombreObligatorio(nombre);
                Curso? curso;

                if (cursoId != null)
                {
                    curso = BuscarCurso(cursoId) ?? throw new KeyNotFoundException("Curso no encontrado.");
                }
                else
                {
                    curso = UsuarioActivo.Cursos.FirstOrDefault();
                    if (curso == null)
                    {
                        curso = new Curso { Nombre = "Mi curso", Siglas = "CURSO" };
                        UsuarioActivo.Cursos.Add(curso);
                    }
                }

                var asignatura = new Asignatura
                {
                    Nombre = nombreLimpio,
                    Siglas = siglas?.Trim() ?? ""
                };
                curso.Asignaturas.Add(asignatura);
                return asignatura;
            });

        public static Task<Asignatura> EditarAsignaturaAsync(
            string id, string nombre, string siglas = "") =>
            CambiarYGuardarAsync(() =>
            {
                Asignatura asignatura = BuscarAsignatura(id)
                    ?? throw new KeyNotFoundException("Asignatura no encontrada.");
                asignatura.Nombre = NombreObligatorio(nombre);
                asignatura.Siglas = siglas?.Trim() ?? "";
                return asignatura;
            });

        public static Task<bool> EliminarAsignaturaAsync(string id) =>
            CambiarYGuardarAsync(() =>
            {
                foreach (Curso curso in UsuarioActivo.Cursos)
                {
                    Asignatura? asignatura = curso.Asignaturas.FirstOrDefault(a => a.Id == id);
                    if (asignatura != null)
                        return curso.Asignaturas.Remove(asignatura);
                }
                return false;
            });

        // --- TEMAS ---

        public static Task<Tema> CrearTemaAsync(string asignaturaId, string nombre) =>
            CambiarYGuardarAsync(() =>
            {
                Asignatura asignatura = BuscarAsignatura(asignaturaId)
                    ?? throw new KeyNotFoundException("Asignatura no encontrada.");
                var tema = new Tema { Nombre = NombreObligatorio(nombre) };
                asignatura.Temas.Add(tema);
                return tema;
            });

        public static Task<Tema> EditarTemaAsync(string id, string nombre) =>
            CambiarYGuardarAsync(() =>
            {
                Tema tema = BuscarTema(id) ?? throw new KeyNotFoundException("Tema no encontrado.");
                tema.Nombre = NombreObligatorio(nombre);
                return tema;
            });

        public static Task<bool> EliminarTemaAsync(string id) =>
            CambiarYGuardarAsync(() =>
            {
                foreach (Asignatura asignatura in ObtenerAsignaturas())
                {
                    Tema? tema = asignatura.Temas.FirstOrDefault(t => t.Id == id);
                    if (tema != null)
                        return asignatura.Temas.Remove(tema);
                }
                return false;
            });

        // --- PREGUNTAS ---

        public static Task<Pregunta> CrearPreguntaAsync(string temaId, Pregunta pregunta) =>
            CambiarYGuardarAsync(() =>
            {
                if (pregunta == null || !pregunta.EsValida())
                    throw new ArgumentException("La pregunta debe tener enunciado, opciones y respuesta correcta válidos.");

                Tema tema = BuscarTema(temaId) ?? throw new KeyNotFoundException("Tema no encontrado.");

                if (BuscarPregunta(pregunta.Id) != null)
                    throw new InvalidOperationException("Ya existe una pregunta con ese identificador.");

                tema.BancoDePreguntas.Add(pregunta);
                return pregunta;
            });

        /// <summary>
        /// Cambia los datos de una pregunta conservando su Id.
        /// nuevoTemaId permite moverla a otro tema si se indica.
        /// </summary>
        public static Task<Pregunta> EditarPreguntaAsync(
            string preguntaId, Pregunta datos, string? nuevoTemaId = null) =>
            CambiarYGuardarAsync(() =>
            {
                if (datos == null || !datos.EsValida())
                    throw new ArgumentException("Los datos de la pregunta no son válidos.");

                Tema origen = BuscarTemaDePregunta(preguntaId)
                    ?? throw new KeyNotFoundException("Pregunta no encontrada.");
                Pregunta original = origen.BancoDePreguntas.First(p => p.Id == preguntaId);
                Tema destino = nuevoTemaId == null
                    ? origen
                    : BuscarTema(nuevoTemaId) ?? throw new KeyNotFoundException("Tema de destino no encontrado.");

                original.Enunciado = datos.Enunciado.Trim();
                original.Opciones = datos.Opciones.Select(o => o.Trim()).ToList();
                original.RespuestaCorrecta = datos.RespuestaCorrecta;
                original.FeedbackExplicacion = datos.FeedbackExplicacion?.Trim() ?? "";

                if (!ReferenceEquals(origen, destino))
                {
                    origen.BancoDePreguntas.Remove(original);
                    destino.BancoDePreguntas.Add(original);
                }

                return original;
            });

        public static Task<bool> EliminarPreguntaAsync(string preguntaId) =>
            CambiarYGuardarAsync(() =>
            {
                Tema? tema = BuscarTemaDePregunta(preguntaId);
                Pregunta? pregunta = tema?.BancoDePreguntas.FirstOrDefault(p => p.Id == preguntaId);
                return pregunta != null && tema!.BancoDePreguntas.Remove(pregunta);
            });

        // --- PERFIL Y CUESTIONARIOS ---

        public static Task<Usuario> ActualizarPerfilAsync(string nombre, string email) =>
            CambiarYGuardarAsync(() =>
            {
                UsuarioActivo.Nombre = NombreObligatorio(nombre);
                UsuarioActivo.Email = email?.Trim() ?? "";
                return UsuarioActivo;
            });

        public static Task<Cuestionario> AgregarCuestionarioAsync(
            string asignaturaId, Cuestionario cuestionario) =>
            CambiarYGuardarAsync(() =>
            {
                ArgumentNullException.ThrowIfNull(cuestionario);
                Asignatura asignatura = BuscarAsignatura(asignaturaId)
                    ?? throw new KeyNotFoundException("Asignatura no encontrada.");
                asignatura.Cuestionarios.Add(cuestionario);
                return cuestionario;
            });

        /// <summary>
        /// Inicializa todos los datos de prueba simulando un estudiante real con historial.
        /// </summary>
        public static void InicializarMockData()
        {
            // Solo para pruebas. No llamarlo al iniciar siempre una aplicación real.
            // Evita duplicar los ejemplos si ya hay cursos cargados.
            if (UsuarioActivo.Cursos.Count > 0) return;

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
