using Xunit;
using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Models;
using DroneDelivery.Domain.Enums;
using System;

namespace DroneDelivery.Tests
{
    // A classe de testes deve ser nomeada com 'Tests' no final
    public class PedidoTests
    {
        [Fact]
        public void Construtor_DeveCriarPedido_ComValoresValidos()
        {
            var localizacao = new Ponto(10, 5);
            double peso = 2.5;
            var prioridade = Prioridade.Media;

            var pedido = new Pedido(localizacao, peso, prioridade);

            // Garante que o objeto foi criado e os valores estão corretos
            Assert.NotNull(pedido);
            Assert.Equal(2.5, pedido.Peso);
        }

        [Fact]
        public void Construtor_DeveLancarExcecao_QuandoPesoForZeroOuNegativo()
        {
            var localizacao = new Ponto(1, 1);

            // Verifica se ArgumentException é lançada com peso 0
            Assert.Throws<ArgumentException>(() => new Pedido(localizacao, 0, Prioridade.Baixa));

            // Verifica se ArgumentException é lançada com peso negativo
            Assert.Throws<ArgumentException>(() => new Pedido(localizacao, -1.0, Prioridade.Baixa));
        }
    }
}