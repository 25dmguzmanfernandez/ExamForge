using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamForge.Models;
using ExamForge.Services;

namespace ExamForge.ViewModels
{
    public partial class TemaViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private Tema _temaActual;

        [ObservableProperty] private string nombreTema;
        [ObservableProperty] private string nuevoEnunciado;
        [ObservableProperty] private string opcion1;
        [ObservableProperty] private string opcion2;
        [ObservableProperty] private string opcion3;
        [ObservableProperty] private string opcion4;
        [ObservableProperty] private string feedbackExplicacion;

        public ObservableCollection<Pregunta> BancoDePreguntas { get; set; } = new();

        public TemaViewModel(DataService dataService)
        {
            _dataService = dataService;
        }

        public void Inicializar(string temaId)
        {
            _temaActual = _dataService.ObtenerUsuarios()
                .SelectMany(u => u.Cursos)
                .SelectMany(c => c.Asignaturas)
                .SelectMany(a => a.Temas)
                .FirstOrDefault(t => t.Id == temaId);

            if (_temaActual == null) return;

            NombreTema = _temaActual.Nombre;
            RefrescarBanco();
        }

        private void RefrescarBanco()
        {
            BancoDePreguntas.Clear();
            foreach (var pregunta in _temaActual.BancoDePreguntas)
            {
                BancoDePreguntas.Add(pregunta);
            }
        }

        [RelayCommand]
        private void AgregarNuevaPregunta()
        {
            var opcionesLista = new List<string> { Opcion1, Opcion2, Opcion3, Opcion4 }
                .Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            var nuevaPregunta = new Pregunta
            {
                Enunciado = NuevoEnunciado,
                Opciones = opcionesLista,
                RespuestaCorrecta = 0,
                FeedbackExplicacion = FeedbackExplicacion
            };

            if (!nuevaPregunta.EsValida()) return;

            _temaActual.BancoDePreguntas.Add(nuevaPregunta);
            _dataService.GuardarDatos();

            RefrescarBanco();
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            NuevoEnunciado = string.Empty;
            Opcion1 = string.Empty;
            Opcion2 = string.Empty;
            Opcion3 = string.Empty;
            Opcion4 = string.Empty;
            FeedbackExplicacion = string.Empty;
        }
    }
}
