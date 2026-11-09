namespace ExamForge.Models
{
    public interface IEstadisticas
    {
        int TotalPreguntasRespondidas { get; }
        int TotalPreguntasAcertadas { get; }
        double PorcentajeAcierto { get; }
    }
}
