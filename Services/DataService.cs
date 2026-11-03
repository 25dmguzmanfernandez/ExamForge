using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ExamForge.Models;

namespace ExamForge.Services
{
    public class DataService
    {
        // Ruta segura y privada para guardar datos en Android, iOS, Windows o Mac
        private readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "data.json");
        private List<Usuario> _usuarios = new List<Usuario>();

        public DataService()
        {
            CargarDatos();
        }

        public List<Usuario> ObtenerUsuarios() => _usuarios;

        public void GuardarDatos()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_usuarios, options);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar JSON: {ex.Message}");
            }
        }

        private void CargarDatos()
        {
            try
            {
                // 1. Si no existe el archivo en la ruta privada de escritura, lo copiamos de los recursos embebidos
                if (!File.Exists(_filePath))
                {
                    // Abrimos el archivo JSON prefabricado de la carpeta Resources/Raw
                    using Stream stream = FileSystem.OpenAppPackageFileAsync("data.json").Result;
                    using StreamReader reader = new StreamReader(stream);
                    string jsonInicial = reader.ReadToEnd();

                    // Lo guardamos en la ruta de lectura/escritura del dispositivo
                    File.WriteAllText(_filePath, jsonInicial);
                }

                // 2. Leemos de forma normal el archivo (que ahora sí existirá seguro)
                string jsonString = File.ReadAllText(_filePath);
                _usuarios = JsonSerializer.Deserialize<List<Usuario>>(jsonString) ?? new List<Usuario>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar la base de datos JSON: {ex.Message}");
                _usuarios = new List<Usuario>();
            }
        }

    }
}
