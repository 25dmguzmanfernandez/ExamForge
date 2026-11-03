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
        private readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "examforge_data.json");
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
                if (File.Exists(_filePath))
                {
                    string jsonString = File.ReadAllText(_filePath);
                    _usuarios = JsonSerializer.Deserialize<List<Usuario>>(jsonString) ?? new List<Usuario>();
                }
                else
                {
                    _usuarios = new List<Usuario>();
                    GuardarDatos();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar JSON: {ex.Message}");
                _usuarios = new List<Usuario>();
            }
        }
    }
}
