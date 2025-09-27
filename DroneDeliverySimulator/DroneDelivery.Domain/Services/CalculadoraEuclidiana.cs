using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Implementa o cálculo da distância Euclidiana (linha reta) entre dois pontos.
    /// </summary>
    public class CalculadoraEuclidiana : ICalculadoraDistancia
    {
        public double CalcularDistancia(Ponto pontoA, Ponto pontoB)
        {
            // Distância Euclidiana: sqrt((x2 - x1)^2 + (y2 - y1)^2)
            double deltaX = pontoB.X - pontoA.X;
            double deltaY = pontoB.Y - pontoA.Y;

            // Retorna a raiz quadrada da soma dos quadrados das diferenças.
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }
    }
}