/*
 * QuesoDrawable.cs: dibuja el gráfico de queso de las cards (Cursos y Asignaturas).
 * Aro exterior azul pálido = total de preguntas; porciones interiores = sin contestar (gris),
 * correctas (verde) e incorrectas (rojo). Se usa con <GraphicsView Drawable="{Binding Queso}"/>.
 * Librería: Microsoft.Maui.Graphics (incluida en MAUI, sin paquetes externos). Las porciones se
 * construyen como polígonos para no depender de los ángulos de AddArc.
 * Establecer(): recibe las cifras que calcula la lógica.
 */
using Microsoft.Maui.Graphics;

namespace ExamForge.Views.Componentes;

public class QuesoDrawable : IDrawable
{
    private int total, sinContestar, correctas, incorrectas;

    public void Establecer(int total, int sinContestar, int correctas, int incorrectas)
    {
        this.total = total;
        this.sinContestar = sinContestar;
        this.correctas = correctas;
        this.incorrectas = incorrectas;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float cx = dirtyRect.Center.X;
        float cy = dirtyRect.Center.Y;
        float radio = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f - 2f;

        // Aro exterior: total de preguntas
        canvas.FillColor = Paleta.AzulPalido;
        canvas.FillCircle(cx, cy, radio);

        float radioInterior = radio * 0.78f;
        int suma = sinContestar + correctas + incorrectas;

        if (total <= 0 || suma <= 0)
        {
            canvas.FillColor = Colors.White;
            canvas.FillCircle(cx, cy, radioInterior);
            return;
        }

        float inicio = -90f; // arriba, sentido horario
        inicio = Porcion(canvas, cx, cy, radioInterior, inicio, sinContestar, suma, Paleta.Gris);
        inicio = Porcion(canvas, cx, cy, radioInterior, inicio, correctas, suma, Paleta.Verde);
        Porcion(canvas, cx, cy, radioInterior, inicio, incorrectas, suma, Paleta.Rojo);
    }

    private static float Porcion(ICanvas canvas, float cx, float cy, float radio,
                                 float inicio, int valor, int suma, Color color)
    {
        if (valor <= 0) return inicio;

        float barrido = 360f * valor / suma;
        var camino = new PathF();
        camino.MoveTo(cx, cy);

        for (float a = 0; a < barrido; a += 4f)
            camino.LineTo(Punto(cx, radio, inicio + a, true), Punto(cy, radio, inicio + a, false));

        camino.LineTo(Punto(cx, radio, inicio + barrido, true), Punto(cy, radio, inicio + barrido, false));
        camino.Close();

        canvas.FillColor = color;
        canvas.FillPath(camino);
        return inicio + barrido;
    }

    private static float Punto(float centro, float radio, float grados, bool esX)
    {
        double rad = grados * Math.PI / 180.0;
        return centro + radio * (float)(esX ? Math.Cos(rad) : Math.Sin(rad));
    }
}
