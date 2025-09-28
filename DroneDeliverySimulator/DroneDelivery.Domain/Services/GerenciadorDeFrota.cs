using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using System; // Adicionado para ArgumentNullException e o novo método Math.Sqrt
using System.Collections.Generic;
using System.Linq;
using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Implementa a lógica central de otimização e alocação de drones.
    /// </summary>
    public class GerenciadorDeFrota : IGerenciadorDeFrota
    {
        private readonly ICalculadoraDistancia _calculadora;
        private readonly List<Drone> _frota;

        private List<Pedido> _pedidosPendentes;

        public IReadOnlyList<Drone> Frota => _frota.AsReadOnly();
        public IReadOnlyList<Pedido> PedidosPendentes => _pedidosPendentes.AsReadOnly();

        public GerenciadorDeFrota(ICalculadoraDistancia calculadora, List<Drone> frotaInicial)
        {
            _calculadora = calculadora;
            _frota = frotaInicial ?? throw new ArgumentNullException(nameof(frotaInicial));
            _pedidosPendentes = new List<Pedido>();
        }

        public IEnumerable<Drone> GetDrones() => _frota;

        /// <summary>
        /// O coração da lógica: Aloca pacotes para drones com o menor número de viagens.
        /// </summary>
        public List<Voo> AlocarPedidos(List<Pedido> pedidosAAlocar)
        {
            var voosPlanejados = new List<Voo>();

            // 1. INICIALIZAÇÃO
            _pedidosPendentes = new List<Pedido>(pedidosAAlocar);

            // 2. Priorização: Ordenar os pedidos.
            var pedidosOrdenados = _pedidosPendentes
                .OrderByDescending(p => p.Prioridade)
                .ThenByDescending(p => p.Peso)
                .ToList();

            var dronesDisponiveis = _frota.Where(d => d.Status == DroneStatus.Idle).ToList();

            // 3. Alocação Gulosa (Greedy Allocation)
            foreach (var drone in dronesDisponiveis)
            {
                var vooAtual = new Voo(drone);
                var pacotesParaRemover = new List<Pedido>();

                var pedidosDisponiveisParaDrone = pedidosOrdenados.Where(p => _pedidosPendentes.Contains(p)).ToList();

                foreach (var pedido in pedidosDisponiveisParaDrone)
                {
                    // Regra de Validação 1: O pacote sozinho é muito pesado?
                    if (pedido.Peso > drone.CapacidadeMaxKg)
                    {
                        continue; // Pacote inválido para este drone
                    }

                    var novoPeso = vooAtual.PesoTotalCarga + pedido.Peso;

                    // Checa Capacidade
                    if (novoPeso <= drone.CapacidadeMaxKg)
                    {
                        // SIMULAÇÃO: Cria um Voo temporário para checar a Distância
                        var vooTemp = new Voo(drone);

                        // Adicionar pacotes já alocados + o novo pedido para calcular a rota
                        // NOTE: Depende do método AdicionarPacoteSemValidacao no Voo.cs
                        vooAtual.Pacotes.ForEach(p => vooTemp.AdicionarPacoteSemValidacao(p));
                        vooTemp.AdicionarPacoteSemValidacao(pedido);

                        // Recalcular a rota e a distância total do Voo Temporário
                        vooTemp.RecalcularDistanciaTotal(_calculadora);

                        // Checa Alcance
                        if (vooTemp.DistanciaTotalRotaKm <= drone.AlcanceMaxKm)
                        {
                            // Aloca o pacote de forma definitiva no voo
                            vooAtual.AdicionarPacote(pedido, _calculadora);
                            pacotesParaRemover.Add(pedido);
                        }
                    }
                }

                // 4. Finalizar o Voo
                if (vooAtual.Pacotes.Any())
                {
                    vooAtual.RecalcularTempoTotal(); // Garante o cálculo do tempo
                    drone.Status = DroneStatus.Carregando;
                    voosPlanejados.Add(vooAtual);

                    // Remove os pedidos alocados da lista da instância
                    _pedidosPendentes.RemoveAll(p => pacotesParaRemover.Contains(p));
                }
            }

            return voosPlanejados;
        }

        // --- NOVO MÉTODO: ANÁLISE DETALHADA DE FALHA ---

        /// <summary>
        /// Analisa um pedido que não pôde ser alocado e identifica o limite excedido (Peso ou Alcance), 
        /// usando o drone de melhor capacidade para fornecer dicas precisas.
        /// </summary>
        public string AnalisarFalhaDeAlocacao(Pedido pedido)
        {
            // Encontrar o drone de maior Alcance (e Capacidade) para ser a referência de limite
            var melhorDrone = _frota
                .OrderByDescending(d => d.AlcanceMaxKm) // Prioriza o maior alcance
                .ThenByDescending(d => d.CapacidadeMaxKg)
                .FirstOrDefault();

            if (melhorDrone == null)
            {
                // Esta situação é improvável após a checagem no CLI, mas é uma segurança.
                return "⚠️ Nenhum drone disponível para análise de limites.";
            }

            // 1. ANÁLISE DO PESO
            if (pedido.Peso > melhorDrone.CapacidadeMaxKg)
            {
                var excedente = pedido.Peso - melhorDrone.CapacidadeMaxKg;
                return $"❌ Peso Excedido: O pacote de {pedido.Peso:F1}kg excede a capacidade máxima ({melhorDrone.CapacidadeMaxKg:F1}kg) por {excedente:F1}kg.";
            }

            // 2. ANÁLISE DO ALCANCE

            // Criar um Voo temporário para medir a distância total de ida e volta (apenas este pacote)
            var vooTeste = new Voo(melhorDrone);

            // Simula a adição do pacote para calcular a rota (Base -> Cliente -> Base)
            // NOTA: Assumindo que você criou o AdicionarPacote(Pedido, ICalculadoraDistancia)
            // que apenas calcula a rota e não lança exceção de alcance.
            vooTeste.AdicionarPacote(pedido, _calculadora);

            // Se o Alcance for excedido
            if (vooTeste.DistanciaTotalRotaKm > melhorDrone.AlcanceMaxKm)
            {
                var excedenteRotaTotal = vooTeste.DistanciaTotalRotaKm - melhorDrone.AlcanceMaxKm;

                // Cálculo da Distância Máxima Aceitável da Base ao Cliente
                // Distância Máx da Base = Alcance Máx / 2
                var maxDistanciaDaBase = melhorDrone.AlcanceMaxKm / 2.0;

                // Distância do cliente até a Base (0,0)
                var distanciaCliente = _calculadora.CalcularDistancia(Ponto.Base, pedido.LocalizacaoCliente);

                return $"❌ Alcance Excedido: Rota total (Ida e Volta) é de {vooTeste.DistanciaTotalRotaKm:F1}km, excedendo o limite de {melhorDrone.AlcanceMaxKm:F1}km por {excedenteRotaTotal:F1}km." +
                       $"\nDICA: O cliente está a {distanciaCliente:F1}km da base. O limite máximo de distância (raio) é {maxDistanciaDaBase:F1}km. " +
                       $"Para ser aceito, as coordenadas do cliente devem resultar em uma distância de até {maxDistanciaDaBase:F1}km da base (0,0).";
            }

            // Se o pacote passou nas checagens individuais de Peso e Alcance, a falha é por falta de drones Idle.
            return "✅ O pedido individualmente é válido, mas não pôde ser alocado. Todos os drones adequados estão ocupados. Tente novamente no próximo ciclo.";
        }
    }

  
}