using System.Text.Json;
using ExamForge.Models;

namespace ExamForge.Services;

/// <summary>
/// Únicamente se encarga de leer y escribir los datos en el archivo JSON.
/// No conoce las páginas ni modifica las colecciones de la interfaz.
/// </summary>
public static class DataStorage
{
    public static string RutaArchivo =>
        Path.Combine(FileSystem.Current.AppDataDirectory, "examforge_datos.json");

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        // No guardar propiedades calculadas como PorcentajeAcierto.
        IgnoreReadOnlyProperties = true
    };

    public static async Task<Usuario?> CargarAsync()
    {
        if (!File.Exists(RutaArchivo))
            return null; // Primera ejecución: todavía no hay archivo.

        string json = await File.ReadAllTextAsync(RutaArchivo);

        // Si el archivo existe pero contiene JSON inválido, lanzamos la excepción.
        // Así nunca lo sobrescribimos silenciosamente con datos vacíos.
        return JsonSerializer.Deserialize<Usuario>(json, OpcionesJson)
            ?? throw new InvalidDataException("El archivo de ExamForge no contiene un usuario válido.");
    }

    public static async Task GuardarAsync(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        string json = JsonSerializer.Serialize(usuario, OpcionesJson);

        // Primero escribimos un archivo temporal y luego lo sustituimos.
        // Evita dejar el JSON principal a medio escribir si falla la operación.
        string temporal = RutaArchivo + ".tmp";
        await File.WriteAllTextAsync(temporal, json);
        File.Move(temporal, RutaArchivo, true);
    }
}
