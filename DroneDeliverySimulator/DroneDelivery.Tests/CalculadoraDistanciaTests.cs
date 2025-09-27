using Xunit;
using DroneDelivery.Domain.Services;
using DroneDelivery.Domain.Models;
using System;

namespace DroneDelivery.Tests
{
    public class CalculadoraDistanciaTests
    {
        // O serviço de cálculo que será testado
        private readonly ICalculadoraDistancia _calculadora = new CalculadoraEuclidiana();

        // Testa a distância básica no eixo X
        [Fact]
        public void CalcularDistancia_DeveRetornarDistanciaCorreta_NoEixoX()
        {
            // Arrange
            var pontoA = new Ponto(0, 0);
            var pontoB = new Ponto(10, 0);

            // Act
            double distancia = _calculadora.CalcularDistancia(pontoA, pontoB);

            // Assert
            Assert.Equal(10, distancia);
        }

        // Testa a distância básica no eixo Y
        [Fact]
        public void CalcularDistancia_DeveRetornarDistanciaCorreta_NoEixoY()
        {
            // Arrange
            var pontoA = new Ponto(0, 0);
            var pontoB = new Ponto(0, 5);

            // Act
            double distancia = _calculadora.CalcularDistancia(pontoA, pontoB);

            // Assert
            Assert.Equal(5, distancia);
        }

        // Testa a distância na diagonal (Teorema de Pitágoras: 3^2 + 4^2 = 5^2)
        [Fact]
        public void CalcularDistancia_DeveRetornarDistanciaCorreta_NaDiagonal()
        {
            // Arrange
            var pontoA = new Ponto(0, 0);
            var pontoB = new Ponto(3, 4);

            // Act
            double distancia = _calculadora.CalcularDistancia(pontoA, pontoB);

            // Assert
            // 5 é o resultado esperado (3*3 + 4*4 = 9 + 16 = 25. Raiz de 25 é 5)
            Assert.Equal(5, distancia);
        }
    }
}