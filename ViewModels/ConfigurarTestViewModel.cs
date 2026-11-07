using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels;

public partial class ConfigurarTestViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty] private Asignatura? asignaturaActual;
    [ObservableProperty] private string cantidadPreguntas = "10";
    [ObservableProperty] private int disponibles;
    [ObservableProperty] private bool puedeIniciar;
    [ObservableProperty] private string mensajeError = "";
    [ObservableProperty] private bool temaFijado;

    public ObservableCollection<TemaSeleccionableDto> TemasParaSeleccionar { get; } = new();

    public string NombreAsignatura => AsignaturaActual?.Nombre ?? "";

    public ConfigurarTestViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public void Inicializar(string asignaturaId, string? temaId = null)
    {
        AsignaturaActual = _dataService.UsuarioActivo?
            .Cursos
            .SelectMany(c => c.Asignaturas)
            .FirstOrDefault(a => a.Id == asignaturaId);

        TemaFijado = !string.IsNullOrWhiteSpace(temaId);
        TemasParaSeleccionar.Clear();

        if (AsignaturaActual is null)
        {
            MensajeError = "No se encontró la asignatura.";
            PuedeIniciar = false;
            return;
        }

        foreach (var tema in AsignaturaActual.Temas)
        {
            if (TemaFijado && tema.Id != temaId) continue;

            var fila = new TemaSeleccionableDto(tema, TemaFijado);
            fila.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TemaSeleccionableDto.IsSelected))
                    ActualizarValidacion();
            };

            TemasParaSeleccionar.Add(fila);
        }

        ActualizarValidacion();
    }

    partial void OnCantidadPreguntasChanged(string value) => ActualizarValidacion();

    public void ActualizarValidacion()
    {
        var elegidos = TemasParaSeleccionar.Where(t => t.IsSelected).ToList();
        Disponibles = elegidos.Sum(t => t.TemaRef.BancoDePreguntas.Count);

        bool cantidadValida =
            int.TryParse(CantidadPreguntas, out int cantidad) &&
            cantidad >= 1 &&
            cantidad <= Disponibles;

        PuedeIniciar = elegidos.Count > 0 && cantidadValida;

        MensajeError = elegidos.Count == 0
            ? "Selecciona al menos un tema."
            : !int.TryParse(CantidadPreguntas, out cantidad) || cantidad < 1
                ? "Indica una cantidad de preguntas válida."
                : cantidad > Disponibles
                    ? $"Solo hay {Disponibles} pregunta(s) disponibles."
                    : "";
    }

    [RelayCommand]
    private async Task IniciarCuestionario()
    {
        ActualizarValidacion();

        if (!PuedeIniciar ||
            AsignaturaActual is null ||
            !int.TryParse(CantidadPreguntas, out int cantidad))
        {
            return;
        }

        var temas = TemasParaSeleccionar
            .Where(t => t.IsSelected)
            .Select(t => t.TemaRef)
            .ToList();

        var cuestionario = new Cuestionario
        {
            CantidadPreguntasSolicitadas = cantidad,
            EsRepartoEquitativo = temas.Count > 1,
            TemasIdsSeleccionados = temas.Select(t => t.Id).ToList(),
            Preguntas = CuestionarioEngine.GenerarPreguntas(
                AsignaturaActual.Temas,
                temas.Select(t => t.Id).ToList(),
                cantidad)
        };

        if (cuestionario.Preguntas.Count != cantidad)
        {
            MensajeError = "No se pudieron seleccionar todas las preguntas solicitadas.";
            return;
        }

        // Se conserva en memoria. El intento no entra al historial hasta finalizar.
        _dataService.AsignaturaEnCurso = AsignaturaActual;
        _dataService.CuestionarioEnCurso = cuestionario;

        await Shell.Current.GoToAsync($"../{nameof(ExamForge.Views.CuestionarioPage)}");
    }
}

public partial class TemaSeleccionableDto : ObservableObject
{
    public Tema TemaRef { get; }

    public string Texto => $"{TemaRef.Nombre} ({TemaRef.BancoDePreguntas.Count})";

    public TemaSeleccionableDto(Tema tema, bool seleccionado)
    {
        TemaRef = tema;
        IsSelected = seleccionado;
    }

    [ObservableProperty] private bool isSelected;
}