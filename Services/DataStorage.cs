using System.Text.Json;
using ExamForge.Models;

namespace ExamForge.Services;

/// <summary>
/// Persistencia local de los datos de ExamForge en un archivo JSON.
/// Guardamos el usuario completo para conservar cursos, asignaturas, temas y preguntas.
/// </summary>
public static class DataStorage
{
    private static readonly string ruta =
        Path.Combine(FileSystem.Current.AppDataDirectory, "examforge_datos.json");

    private static readonly SemaphoreSlim semaforo = new(1, 1);
    private static bool cargado = false;

    private static readonly JsonSerializerOptions opciones = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true
    };

    public static async Task CargarAsync()
    {
        await semaforo.WaitAsync();
        try
        {
            if (cargado) return;

            if (File.Exists(ruta))
            {
                string json = await File.ReadAllTextAsync(ruta);
                Usuario usuario = JsonSerializer.Deserialize<Usuario>(json) ?? new Usuario();
                DataService.UsuarioActivo = usuario;
            }

            // Solo se marca como cargado tras completar la lectura.
            // Si falla, podrá intentarse de nuevo sin sobrescribir el JSON antiguo.
            cargado = true;
        }
        finally
        {
            semaforo.Release();
        }
    }

    public static async Task GuardarAsync()
    {
        // Impide sobrescribir los datos anteriores antes de haberlos cargado.
        await CargarAsync();

        await semaforo.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(DataService.UsuarioActivo, opciones);
            string temporal = ruta + ".tmp";
            await File.WriteAllTextAsync(temporal, json);
            File.Move(temporal, ruta, true);
        }
        finally
        {
            semaforo.Release();
        }
    }
}
