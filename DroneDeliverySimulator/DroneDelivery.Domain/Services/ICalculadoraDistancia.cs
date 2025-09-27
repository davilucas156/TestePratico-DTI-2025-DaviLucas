using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Define o contrato para serviços que calculam distâncias entre pontos.
    /// Isso permite trocar a lógica de cálculo (ex: Euclidiana para Manhattan) facilmente.
    /// </summary>
    public interface ICalculadoraDistancia
    {
        /// <summary>
        /// Calcula a distância entre dois pontos (A e B).
        /// </summary>
        /// <returns>A distância calculada em km.</returns>
        double CalcularDistancia(Ponto pontoA, Ponto pontoB);
    }
}