using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using System; 
using System.Collections.Generic;
using System.Linq;
using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Services
{
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

       //logica principal 
        public List<Voo> AlocarPedidos(List<Pedido> pedidosAAlocar)
        {
            var voosPlanejados = new List<Voo>();

            _pedidosPendentes = new List<Pedido>(pedidosAAlocar);

            // Priorização
            var pedidosOrdenados = _pedidosPendentes
                .OrderByDescending(p => p.Prioridade)
                .ThenByDescending(p => p.Peso)
                .ToList();

            var dronesDisponiveis = _frota.Where(d => d.Status == DroneStatus.Idle).ToList();

            // alocação gulosa
            foreach (var drone in dronesDisponiveis)
            {
                var vooAtual = new Voo(drone);
                var pacotesParaRemover = new List<Pedido>();

                var pedidosDisponiveisParaDrone = pedidosOrdenados.Where(p => _pedidosPendentes.Contains(p)).ToList();

                foreach (var pedido in pedidosDisponiveisParaDrone)
                {
                    if (pedido.Peso > drone.CapacidadeMaxKg)
                    {
                        continue; 
                    }

                    var novoPeso = vooAtual.PesoTotalCarga + pedido.Peso;

                    if (novoPeso <= drone.CapacidadeMaxKg)
                    {
                        var vooTemp = new Voo(drone);

                        vooAtual.Pacotes.ForEach(p => vooTemp.AdicionarPacoteSemValidacao(p));
                        vooTemp.AdicionarPacoteSemValidacao(pedido);

                        vooTemp.RecalcularDistanciaTotal(_calculadora);
                        var alcanceEfetivo = drone.GetAlcanceEfetivoKm(vooTemp.PesoTotalCarga);


                        if (vooTemp.DistanciaTotalRotaKm <= alcanceEfetivo)
                        {
                            vooAtual.AdicionarPacote(pedido, _calculadora);
                            pacotesParaRemover.Add(pedido);
                        }
                    }
                }

                if (vooAtual.Pacotes.Any())
                {
                    vooAtual.RecalcularTempoTotal(); 
                    drone.Status = DroneStatus.Carregando;
                    voosPlanejados.Add(vooAtual);

                    _pedidosPendentes.RemoveAll(p => pacotesParaRemover.Contains(p));
                }
            }

            return voosPlanejados;
        }


        public string AnalisarFalhaDeAlocacao(Pedido pedido)
        {
            var melhorDrone = _frota
                .OrderByDescending(d => d.CapacidadeMaxKg) // Encontra o drone mais forte como referência
                .FirstOrDefault();

            if (melhorDrone == null)
            {
                return "⚠️ Nenhum drone disponível para análise de limites.";
            }

            
            var alcanceEfetivoMaximo = melhorDrone.GetAlcanceEfetivoKm(pedido.Peso);

            // 1. ANÁLISE DO PESO
            if (pedido.Peso > melhorDrone.CapacidadeMaxKg)
            {
                var excedente = pedido.Peso - melhorDrone.CapacidadeMaxKg;
                return $"❌ Peso Excedido: O pacote de {pedido.Peso:F1}kg excede a capacidade máxima ({melhorDrone.CapacidadeMaxKg:F1}kg) por {excedente:F1}kg.";
            }

            // 2. ANÁLISE DO ALCANCE

            var vooTeste = new Voo(melhorDrone);

            vooTeste.AdicionarPacote(pedido, _calculadora);

            
            if (vooTeste.DistanciaTotalRotaKm > alcanceEfetivoMaximo)
            {
                var excedenteRotaTotal = vooTeste.DistanciaTotalRotaKm - alcanceEfetivoMaximo;

                var maxDistanciaDaBase = alcanceEfetivoMaximo / 2.0;

                var distanciaCliente = _calculadora.CalcularDistancia(Ponto.Base, pedido.LocalizacaoCliente);

                return $"❌ Alcance Excedido: Rota total (Ida e Volta) é de {vooTeste.DistanciaTotalRotaKm:F1}km, excedendo o limite EFETIVO de {alcanceEfetivoMaximo:F1}km (para este peso) por {excedenteRotaTotal:F1}km." +
                       $"\nDICA: O cliente está a {distanciaCliente:F1}km da base. O limite EFETIVO de distância (raio) é {maxDistanciaDaBase:F1}km. " +
                       $"Para ser aceito, as coordenadas do cliente devem resultar em uma distância de até {maxDistanciaDaBase:F1}km da base (0,0).";
            }

            return "✅ O pedido individualmente é válido, mas não pôde ser alocado. Todos os drones adequados estão ocupados. Tente novamente no próximo ciclo.";
        }
    }

  
}