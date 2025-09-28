using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Services
{
    
    public class CalculadoraEuclidiana : ICalculadoraDistancia
    {
        public double CalcularDistancia(Ponto pontoA, Ponto pontoB)
        {
            // Distância Euclidiana: sqrt((x2 - x1)^2 + (y2 - y1)^2)
            double deltaX = pontoB.X - pontoA.X;
            double deltaY = pontoB.Y - pontoA.Y;

            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }
    }
}