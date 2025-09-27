using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
// Certifique-se de que o namespace System está no topo para usar ArgumentNullException

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Implementa a lógica central de otimização e alocação de drones.
    /// </summary>
    public class GerenciadorDeFrota : IGerenciadorDeFrota
    {
        private readonly ICalculadoraDistancia _calculadora;
        private readonly List<Drone> _frota;

        // CORREÇÃO 1: O campo deve ser PRIVATE para encapsulamento e para usar a propriedade
        // O "private" abaixo estava faltando ou incompleto no seu código anterior.
        private List<Pedido> _pedidosPendentes;

        // Propriedade pública para testes (e para o CLI)
        // Corrigido para retornar IReadOnlyList. Não há modificadores extras aqui.
        public IReadOnlyList<Drone> Frota => _frota.AsReadOnly();

        // Propriedade para o CLI acessar os pedidos que sobraram após a alocação
        public IReadOnlyList<Pedido> PedidosPendentes => _pedidosPendentes.AsReadOnly();

        public GerenciadorDeFrota(ICalculadoraDistancia calculadora, List<Drone> frotaInicial)
        {
            _calculadora = calculadora;
            _frota = frotaInicial ?? throw new ArgumentNullException(nameof(frotaInicial));
            // Inicializa a lista de pedidos pendentes no construtor
            _pedidosPendentes = new List<Pedido>();
        }

        public IEnumerable<Drone> GetDrones() => _frota;

        /// <summary>
        /// O coração da lógica: Aloca pacotes para drones com o menor número de viagens.
        /// </summary>
        public List<Voo> AlocarPedidos(List<Pedido> pedidosAAlocar)
        {
            var voosPlanejados = new List<Voo>();

            // 1. INICIALIZAÇÃO: Coloca todos os pedidos passados na lista de pendentes da instância.
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

                // Filtra os pedidos ordenados que ainda estão na lista de pendentes global
                var pedidosDisponiveisParaDrone = pedidosOrdenados.Where(p => _pedidosPendentes.Contains(p)).ToList();

                foreach (var pedido in pedidosDisponiveisParaDrone)
                {
                    // Regra de Validação 1: O pacote sozinho é muito pesado?
                    if (pedido.Peso > drone.CapacidadeMaxKg)
                    {
                        continue; // Pacote inválido para este drone
                    }

                    // --- DECLARAÇÃO NECESSÁRIA 1: NOVO PESO ---
                    // Calcula o peso que o voo teria se este pedido fosse adicionado.
                    var novoPeso = vooAtual.PesoTotalCarga + pedido.Peso;

                    // Checa Capacidade: Se o novo pacote couber na capacidade
                    if (novoPeso <= drone.CapacidadeMaxKg)
                    {
                        // --- DECLARAÇÃO NECESSÁRIA 2: VOO TEMPORÁRIO ---
                        // SIMULAÇÃO: Cria um Voo temporário para checar a Distância
                        var vooTemp = new Voo(drone);

                        // Adicionar pacotes já alocados + o novo pedido para calcular a rota
                        // NOTA: É necessário ter o método AdicionarPacoteSemValidacao no Voo.cs
                        vooAtual.Pacotes.ForEach(p => vooTemp.AdicionarPacoteSemValidacao(p));
                        vooTemp.AdicionarPacoteSemValidacao(pedido);

                        // Recalcular a rota e a distância total do Voo Temporário
                        vooTemp.RecalcularDistanciaTotal(_calculadora);

                        // Checa Alcance: A nova rota excede o alcance máximo do drone?
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
                    drone.Status = DroneStatus.Carregando; // Altera o status do drone
                    voosPlanejados.Add(vooAtual);

                    // Remove os pedidos alocados da lista da instância
                    _pedidosPendentes.RemoveAll(p => pacotesParaRemover.Contains(p));
                }
            }

            return voosPlanejados;
        }
    }

    // Extensão para simplificar a validação no Voo.cs (necessário refatorar Pedido e Voo)
    // Para simplificar a cópia, você precisará adicionar este método ao Voo.cs:
    /*
    public void AdicionarPacoteSemValidacao(Pedido pedido)
    {
        Pacotes.Add(pedido);
        PesoTotalCarga += pedido.Peso;
        RotaPlanejada.Add(pedido.LocalizacaoCliente);
        // Não recalcula distância para otimizar
    }
    */
}