/*
 * Paleta.cs: lee desde C# los colores definidos en ExamForgeStyles.xaml (los XAML usan StaticResource).
 * Obtener(clave, respaldo): devuelve el color del diccionario o el de respaldo si no existe.
 * Nota: se usa TryGetValue porque el indexador Resources["x"] no busca en diccionarios combinados.
 */
using Microsoft.Maui.Graphics;

namespace ExamForge.Views.Componentes;

public static class Paleta
{
    public static Color Obtener(string clave, Color respaldo)
    {
        if (Application.Current != null
            && Application.Current.Resources.TryGetValue(clave, out object? valor)
            && valor is Color color)
            return color;

        return respaldo;
    }

    public static Color AzulOscuro => Obtener("AzulOscuro", Colors.DarkBlue);
    public static Color Texto => Obtener("Texto", Colors.Black);
    public static Color Verde => Obtener("Verde", Colors.SeaGreen);
    public static Color Rojo => Obtener("Error", Colors.Firebrick);
    public static Color Gris => Obtener("Gris", Colors.Gray);
    public static Color AzulPalido => Obtener("AzulPalido", Colors.LightSteelBlue);
}
