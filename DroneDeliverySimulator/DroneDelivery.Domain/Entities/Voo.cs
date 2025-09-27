using DroneDelivery.Domain.Models;
using DroneDelivery.Domain.Services;
using System.Linq;
using System;

namespace DroneDelivery.Domain.Entities
{
    /// <summary>
    /// Representa uma única viagem planejada e alocada para um Drone.
    /// </summary>
    public class Voo
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Drone DroneAlocado { get; }
        public List<Pedido> Pacotes { get; } = new List<Pedido>();

        public double PesoTotalCarga { get; private set; }
        public double DistanciaTotalRotaKm { get; private set; }

        public double TempoTotalEstimadoMinutos { get; private set; }

        public double TempoDecorridoNoVooMinutos { get; set; }


        // Opcional: A sequência de paradas (Base -> Cliente1 -> Cliente2 -> ... -> Base)
        public List<Ponto> RotaPlanejada { get; private set; } = new List<Ponto>();

        /// <summary>
        /// Construtor que inicia um Voo alocado a um drone específico.
        /// </summary>
        public Voo(Drone drone)
        {
            DroneAlocado = drone;
            // O voo começa na base (0,0)
            RotaPlanejada.Add(Ponto.Base);
        }

        /// <summary>
        /// Adiciona um pacote ao voo e atualiza os totais.
        /// </summary>
        /// <param name="pedido">O pacote a ser adicionado.</param>
        /// <param name="calculadora">O serviço de cálculo de distância para recalcular a rota.</param>
        public void AdicionarPacote(Pedido pedido, ICalculadoraDistancia calculadora)
        {
            // Validação de erro essencial:
            if (PesoTotalCarga + pedido.Peso > DroneAlocado.CapacidadeMaxKg)
            {
                throw new InvalidOperationException($"O pacote excede a capacidade máxima do {DroneAlocado.Nome}.");
            }

            Pacotes.Add(pedido);
            PesoTotalCarga += pedido.Peso;

            // Recalcula a rota ideal (simplificadamente, apenas adicionamos o ponto)
            // Em uma otimização real, você procuraria o melhor ponto de inserção.
            RotaPlanejada.Add(pedido.LocalizacaoCliente);

            // Recalcular a Distância Total (da Base até todos os pontos e de volta para a Base)
            RecalcularDistanciaTotal(calculadora);

            // Validação de erro: Checar se a rota total excede o alcance máximo
            if (DistanciaTotalRotaKm > DroneAlocado.AlcanceMaxKm)
            {
                // Se exceder, este pacote não pode ser adicionado.
                // Na lógica de otimização, isso deve ser checado ANTES de chamar este método.
                // Aqui, lançamos uma exceção para indicar a falha.
                throw new InvalidOperationException($"A rota com este pacote excede o alcance máximo ({DroneAlocado.AlcanceMaxKm}km).");
            }
            RecalcularTempoTotal();

        }

        /// <summary>
        /// Recalcula a distância total da rota, assumindo Base -> Pontos de Entrega -> Base.
        /// </summary>
        internal void RecalcularDistanciaTotal(ICalculadoraDistancia calculadora)
        {
            DistanciaTotalRotaKm = 0;
            Ponto pontoAnterior = Ponto.Base;

            // O percurso inclui a base, todos os pontos de entrega e o retorno à base.
            var rotaCompleta = new List<Ponto>(RotaPlanejada);

            // Garante que o último ponto é a base para o retorno
            if (rotaCompleta.Last().X != Ponto.Base.X || rotaCompleta.Last().Y != Ponto.Base.Y)
            {
                rotaCompleta.Add(Ponto.Base);
            }

            for (int i = 1; i < rotaCompleta.Count; i++)
            {
                DistanciaTotalRotaKm += calculadora.CalcularDistancia(rotaCompleta[i - 1], rotaCompleta[i]);
            }
        }

        public void RecalcularTempoTotal()
        {
            // Usamos a fórmula: Tempo (min) = (Distância / Velocidade) * 60
            if (DroneAlocado.VelocidadeMediaKmh > 0)
            {
                TempoTotalEstimadoMinutos = (DistanciaTotalRotaKm / DroneAlocado.VelocidadeMediaKmh) * 10.0;
            }
            else
            {
                // Se a velocidade for zero, define um tempo alto para evitar que o voo termine imediatamente.
                TempoTotalEstimadoMinutos = 99999.0;
            }
        }

        // Dentro da classe Voo.cs

        /// <summary>
        /// Adiciona um pacote **apenas para fins de cálculo temporário**, sem checar alcance ou atualizar status do drone.
        /// </summary>
        internal void AdicionarPacoteSemValidacao(Pedido pedido)
        {
            Pacotes.Add(pedido);
            PesoTotalCarga += pedido.Peso;
            RotaPlanejada.Add(pedido.LocalizacaoCliente);
        }
    }
}