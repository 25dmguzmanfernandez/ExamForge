using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class NuevaPreguntaViewModel : ObservableObject
{
    private readonly DataService _dataService;
    private Tema? _temaActual;
    private Pregunta? _preguntaActual;

    [ObservableProperty] private string nuevoEnunciado = "";
    [ObservableProperty] private string opcion1 = "";
    [ObservableProperty] private string opcion2 = "";
    [ObservableProperty] private string opcion3 = "";
    [ObservableProperty] private string opcion4 = "";
    [ObservableProperty] private string mensajeError = "";
    [ObservableProperty] private int cantidadOpciones = 2;
    [ObservableProperty] private bool guardado;

    public NuevaPreguntaViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string temaId, string? preguntaId = null)
    {
        _temaActual = _dataService.UsuarioActivo?
            .Cursos
            .SelectMany(c => c.Asignaturas)
            .SelectMany(a => a.Temas)
            .FirstOrDefault(t => t.Id == temaId);

        _preguntaActual = _temaActual?.BancoDePreguntas
            .FirstOrDefault(p => p.Id == preguntaId);

        MensajeError = "";
        Guardado = false;

        if (_temaActual is null)
        {
            MensajeError = "No se encontró el tema.";
            return;
        }

        if (_preguntaActual is null)
        {
            LimpiarFormulario();
            return;
        }

        NuevoEnunciado = _preguntaActual.Enunciado;

        var opcionesOrdenadas = new List<string>();
        if (_preguntaActual.RespuestaCorrecta >= 0 &&
            _preguntaActual.RespuestaCorrecta < _preguntaActual.Opciones.Count)
        {
            opcionesOrdenadas.Add(_preguntaActual.Opciones[_preguntaActual.RespuestaCorrecta]);
            opcionesOrdenadas.AddRange(
                _preguntaActual.Opciones.Where((_, i) => i != _preguntaActual.RespuestaCorrecta));
        }

        CantidadOpciones = Math.Clamp(opcionesOrdenadas.Count, 2, 4);
        Opcion1 = ObtenerOpcion(opcionesOrdenadas, 0);
        Opcion2 = ObtenerOpcion(opcionesOrdenadas, 1);
        Opcion3 = ObtenerOpcion(opcionesOrdenadas, 2);
        Opcion4 = ObtenerOpcion(opcionesOrdenadas, 3);
    }

    [RelayCommand]
    private void Guardar()
    {
        MensajeError = "";
        Guardado = false;

        if (_temaActual is null)
        {
            MensajeError = "No se encontró el tema.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NuevoEnunciado))
        {
            MensajeError = "El enunciado es obligatorio.";
            return;
        }

        var opciones = new[] { Opcion1, Opcion2, Opcion3, Opcion4 }
            .Take(CantidadOpciones)
            .Select(o => o.Trim())
            .ToList();

        if (CantidadOpciones is < 2 or > 4 || opciones.Any(string.IsNullOrWhiteSpace))
        {
            MensajeError = "Rellena todas las respuestas visibles. Debe haber entre 2 y 4.";
            return;
        }

        var pregunta = _preguntaActual ?? new Pregunta();

        pregunta.Enunciado = NuevoEnunciado.Trim();
        pregunta.Opciones = opciones;
        pregunta.RespuestaCorrecta = 0;

        if (_preguntaActual is null)
            _temaActual.BancoDePreguntas.Add(pregunta);

        _dataService.GuardarDatos();
        Guardado = true;
    }

    private void LimpiarFormulario()
    {
        NuevoEnunciado = "";
        Opcion1 = "";
        Opcion2 = "";
        Opcion3 = "";
        Opcion4 = "";
        CantidadOpciones = 2;
    }

    public void AgregarPreguntas(IEnumerable<Pregunta> preguntas)
    {
        if (_temaActual is null) return;

        _temaActual.BancoDePreguntas.AddRange(preguntas);
        _dataService.GuardarDatos();
    }

    private static string ObtenerOpcion(IReadOnlyList<string> opciones, int indice) =>
        indice < opciones.Count ? opciones[indice] : "";
}