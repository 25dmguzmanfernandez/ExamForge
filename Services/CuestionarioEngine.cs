using System;
using System.Collections.Generic;
using System.Linq;
using ExamForge.Models;

namespace ExamForge.Services
{
    public static class CuestionarioEngine
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Genera una lista de preguntas aleatorias basándose en los temas seleccionados
        /// y distribuyendo la cantidad total de forma equitativa.
        /// </summary>
        public static List<Pregunta> GenerarPreguntas(List<Tema> todosLosTemas, List<string> temasIdsSeleccionados, int cantidadTotal)
        {
            var preguntasFinales = new List<Pregunta>();

            // Filtrar solo los temas que el usuario seleccionó
            var temasSeleccionados = todosLosTemas.Where(t => temasIdsSeleccionados.Contains(t.Id)).ToList();
            if (!temasSeleccionados.Any() || cantidadTotal <= 0) return preguntasFinales;

            int cantidadTemas = temasSeleccionados.Count;

            // CASO 1: Un solo tema seleccionado (Preguntas aleatorias de ese tema)
            if (cantidadTemas == 1)
            {
                var banco = temasSeleccionados.First().BancoDePreguntas.Where(p => p.EsValida()).ToList();
                return banco
                    .OrderBy(_ => _random.Next())
                    .Take(cantidadTotal)
                    .Select(CopiarConOpcionesBarajadas)
                    .ToList();
            }

            // CASO 2: Múltiples temas - Reparto Equitativo
            int preguntasPorTema = cantidadTotal / cantidadTemas;
            int residuo = cantidadTotal % cantidadTemas;

            for (int i = 0; i < temasSeleccionados.Count; i++)
            {
                var tema = temasSeleccionados[i];
                var bancoTema = tema.BancoDePreguntas.Where(p => p.EsValida()).ToList();

                // Si la división no es exacta, los primeros temas se llevan una pregunta del residuo
                int cuotaAsignada = preguntasPorTema + (i < residuo ? 1 : 0);

                // Tomamos de forma aleatoria respetando el máximo disponible en el banco de ese tema
                var preguntasAleatorias = bancoTema
                    .OrderBy(_ => _random.Next())
                    .Take(cuotaAsignada)
                    .ToList();

                preguntasFinales.AddRange(preguntasAleatorias);
            }

            // Si falta contenido en los temas seleccionados y no se llegó al total por cuotas, 
            // rellenamos con cualquier pregunta válida restante de esos mismos temas
            if (preguntasFinales.Count < cantidadTotal)
            {
                var todasLasDisponibles = temasSeleccionados
                    .SelectMany(t => t.BancoDePreguntas)
                    .Where(p => p.EsValida() && !preguntasFinales.Any(pf => pf.Id == p.Id))
                    .OrderBy(_ => _random.Next());

                int faltantes = cantidadTotal - preguntasFinales.Count;
                preguntasFinales.AddRange(todasLasDisponibles.Take(faltantes));
            }

            // Barajamos el examen completo final para que no salgan ordenadas por bloques de temas
            return preguntasFinales
                .OrderBy(_ => _random.Next())
                .Select(CopiarConOpcionesBarajadas)
                .ToList();
        }

        private static Pregunta CopiarConOpcionesBarajadas(Pregunta original)
        {
            var indices = Enumerable.Range(0, original.Opciones.Count).ToArray();

            // Barajado Fisher-Yates.
            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            return new Pregunta
            {
                Id = original.Id,
                Enunciado = original.Enunciado,
                FeedbackExplicacion = original.FeedbackExplicacion,
                Opciones = indices.Select(i => original.Opciones[i]).ToList(),
                RespuestaCorrecta = Array.IndexOf(indices, original.RespuestaCorrecta)
            };
        }
    }
}
