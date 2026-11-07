/*
 * Provisional.cs  //BORRAR DESPUES
 * Lógica TEMPORAL mínima para poder probar las vistas sin controladores. NO es parte de Views:
 * cuando los compañeros entreguen su lógica, cada llamada a Provisional.* de las páginas se sustituye
 * por su método equivalente (están marcadas con "TODO controlador") y este archivo se elimina.
 * Contiene: usuario falso (Preferences), CRUD en memoria sobre DataService.UsuarioActivo, cifras del
 * gráfico, creación de cuestionario (opciones barajadas con RespuestaCorrecta ajustada) y nota sobre 10.
 * No guarda en disco: la persistencia (DataStorage) la conectará el responsable.
 */
using System.Security.Cryptography;
using System.Text;
using ExamForge.Models;

namespace ExamForge.Views;

public static class Provisional
{
    public static Usuario Usuario { get; set; } = new Usuario();

    /// <summary>True tras cerrar sesión: el portal muestra el estado con los botones Iniciar sesión / Crear usuario.</summary>
    public static bool SesionCerrada { get; set; }

    // Descripciones de cursos y asignaturas (el modelo aún no tiene Descripcion). TODO modelo: Curso.Descripcion y Asignatura.Descripcion
    private static readonly Dictionary<string, string> descripciones = new();
    public static string Descripcion(string id) => descripciones.TryGetValue(id, out var d) ? d : "";

    // ---------- Usuario (TODO modelo: Usuario.Apellidos y contraseña; TODO controlador: registro/login/perfil) ----------
    public static bool HayUsuario => Preferences.Default.ContainsKey("usuario_nombre");

    public static void Registrar(string nombre, string apellidos, string correo, string clave)
    {
        Preferences.Default.Set("usuario_nombre", nombre);
        Preferences.Default.Set("usuario_apellidos", apellidos);
        Preferences.Default.Set("usuario_correo", correo);
        Preferences.Default.Set("usuario_clave", Hash(clave));
        Usuario = new Usuario { Nombre = nombre, Email = correo };
    }

    public static bool Entrar(string nombre, string clave)
    {
        string guardado = Preferences.Default.Get("usuario_nombre", "");
        if (!string.Equals(nombre, guardado, StringComparison.OrdinalIgnoreCase)
            || Hash(clave) != Preferences.Default.Get("usuario_clave", ""))
            return false;

        Usuario.Nombre = guardado;
        Usuario.Email = Preferences.Default.Get("usuario_correo", "");
        SesionCerrada = false;
        return true;
    }

    public static (string Nombre, string Apellidos, string Correo) ObtenerPerfil() =>
        (Preferences.Default.Get("usuario_nombre", ""),
         Preferences.Default.Get("usuario_apellidos", ""),
         Preferences.Default.Get("usuario_correo", ""));

    public static void GuardarPerfil(string nombre, string apellidos, string correo, string claveNueva)
    {
        Preferences.Default.Set("usuario_nombre", nombre);
        Preferences.Default.Set("usuario_apellidos", apellidos);
        Preferences.Default.Set("usuario_correo", correo);
        if (!string.IsNullOrEmpty(claveNueva))
            Preferences.Default.Set("usuario_clave", Hash(claveNueva));
        Usuario.Nombre = nombre;
        Usuario.Email = correo;
    }

    public static void CerrarSesion() => SesionCerrada = true;

    private static string Hash(string texto) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));

    // ---------- Cursos / asignaturas / temas / preguntas (TODO controlador: altas, ediciones y bajas) ----------
    public static void AgregarCurso(string nombre, string descripcion)
    {
        var curso = new Curso { Nombre = nombre, Siglas = Siglas(nombre) };
        descripciones[curso.Id] = descripcion;
        Usuario.Cursos.Add(curso);
    }

    public static void EditarCurso(Curso curso, string nombre, string descripcion)
    {
        curso.Nombre = nombre;
        curso.Siglas = Siglas(nombre);
        descripciones[curso.Id] = descripcion;
    }

    public static void EliminarCursos(IEnumerable<Curso> lista)
    {
        foreach (var curso in lista.ToList()) Usuario.Cursos.Remove(curso);
    }

    public static void AgregarAsignatura(Curso curso, string nombre, string descripcion)
    {
        var asignatura = new Asignatura { Nombre = nombre, Siglas = Siglas(nombre) };
        descripciones[asignatura.Id] = descripcion;
        curso.Asignaturas.Add(asignatura);
    }

    public static void EditarAsignatura(Asignatura asignatura, string nombre, string descripcion)
    {
        asignatura.Nombre = nombre;
        asignatura.Siglas = Siglas(nombre);
        descripciones[asignatura.Id] = descripcion;
    }

    public static void EliminarAsignaturas(Curso curso, IEnumerable<Asignatura> lista)
    {
        foreach (var asignatura in lista.ToList()) curso.Asignaturas.Remove(asignatura);
    }

    public static void AgregarTema(Asignatura asignatura, string nombre) =>
        asignatura.Temas.Add(new Tema { Nombre = nombre });

    public static void AgregarPregunta(Tema tema, Pregunta pregunta) => tema.BancoDePreguntas.Add(pregunta);

    public static void EditarPregunta(Pregunta pregunta, string enunciado, List<string> opciones)
    {
        pregunta.Enunciado = enunciado;
        pregunta.Opciones = opciones;
        pregunta.RespuestaCorrecta = 0; // la primera opción introducida es la correcta
    }

    public static void EliminarPreguntas(Tema tema, IEnumerable<Pregunta> lista)
    {
        foreach (var pregunta in lista.ToList()) tema.BancoDePreguntas.Remove(pregunta);
    }

    // Siglas provisionales: el diseño dice que las deriva la lógica a partir del nombre
    private static string Siglas(string nombre)
    {
        var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(p => p.Length > 2).ToList();
        if (palabras.Count == 0) return nombre.Trim().ToUpper();
        if (palabras.Count == 1) return palabras[0][..Math.Min(4, palabras[0].Length)].ToUpper();
        return string.Concat(palabras.Take(5).Select(p => char.ToUpper(p[0])));
    }

    // ---------- Gráfico de queso (TODO modelo: cifras por curso, asignatura y tema) ----------
    /// <summary>Total en el banco, sin contestar, correctas e incorrectas (según la última vez que se respondió cada pregunta).</summary>
    public static (int Total, int SinContestar, int Correctas, int Incorrectas) Cifras(
        IEnumerable<Tema> temas, IEnumerable<Cuestionario> historico)
    {
        var ids = temas.SelectMany(t => t.BancoDePreguntas).Select(p => p.Id).ToHashSet();
        var ultimo = new Dictionary<string, bool>();

        foreach (var cuestionario in historico.OrderBy(c => c.FechaCreacion))
            foreach (var pregunta in cuestionario.Preguntas)
                if (ids.Contains(pregunta.Id) && cuestionario.RespuestasUsuario.TryGetValue(pregunta.Id, out int r))
                    ultimo[pregunta.Id] = r == pregunta.RespuestaCorrecta;

        int correctas = ultimo.Values.Count(v => v);
        return (ids.Count, ids.Count - ultimo.Count, correctas, ultimo.Count - correctas);
    }

    // ---------- Cuestionario (TODO controlador: generar, calcular nota y guardar el intento) ----------
    /// <summary>Elige preguntas al azar de los temas y devuelve COPIAS con las opciones barajadas y RespuestaCorrecta ajustada.</summary>
    public static Cuestionario CrearCuestionario(IEnumerable<Tema> temas, int cantidad)
    {
        var azar = new Random();
        var cuestionario = new Cuestionario
        {
            CantidadPreguntasSolicitadas = cantidad,
            TemasIdsSeleccionados = temas.Select(t => t.Id).ToList()
        };

        var elegidas = temas.SelectMany(t => t.BancoDePreguntas).OrderBy(_ => azar.Next()).Take(cantidad);
        foreach (var original in elegidas)
        {
            var orden = Enumerable.Range(0, original.Opciones.Count).OrderBy(_ => azar.Next()).ToList();
            cuestionario.Preguntas.Add(new Pregunta
            {
                Id = original.Id,
                Enunciado = original.Enunciado,
                FeedbackExplicacion = original.FeedbackExplicacion,
                Opciones = orden.Select(i => original.Opciones[i]).ToList(),
                RespuestaCorrecta = orden.IndexOf(original.RespuestaCorrecta)
            });
        }
        return cuestionario;
    }

    public static void GuardarCuestionario(Asignatura asignatura, Cuestionario cuestionario) =>
        asignatura.Cuestionarios.Add(cuestionario);

    public static double Nota(Cuestionario cuestionario) => Math.Round(cuestionario.PorcentajeAcierto / 10, 1);
}
