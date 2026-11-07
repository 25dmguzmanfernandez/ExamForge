/*
 * Validar.cs: comprobaciones sencillas de formularios (solo de interfaz).
 * TextoSeguro: rechaza texto con etiquetas o patrones típicos de código inyectado (<, >, javascript:...).
 * CorreoValido: formato básico usuario@dominio.ext.
 * Las contraseñas no se filtran con TextoSeguro (pueden llevar cualquier símbolo).
 */
using System.Text.RegularExpressions;

namespace ExamForge.Services;

public static class Validar
{
    private static readonly string[] Prohibidos = { "<", ">", "javascript:", "onerror", "onload" };

    public static bool TextoSeguro(string texto) =>
        !Prohibidos.Any(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase));

    public static bool CorreoValido(string correo) =>
        Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
