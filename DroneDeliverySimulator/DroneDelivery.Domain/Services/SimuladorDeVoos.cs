using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Gerencia o ciclo de vida dos voos e a transição de estado dos drones.
    /// </summary>
    public class SimuladorDeVoos : ISimuladorDeVoos
    {
        // Armazena os voos que ainda não foram concluídos
        private readonly List<Voo> _voosEmAndamento = new List<Voo>();

        public void IniciarVoos(List<Voo> voos)
        {
            if (voos == null || !voos.Any())
            {
                return;
            }

            foreach (var voo in voos)
            {
                // Garante que o drone está em um estado válido para iniciar o voo
                if (voo.DroneAlocado.Status == DroneStatus.Carregando)
                {
                    // 1. Muda o estado do drone
                    voo.DroneAlocado.Status = DroneStatus.EmVoo;

                    // 2. Adiciona à lista de voos a serem processados
                    _voosEmAndamento.Add(voo);

                    Console.WriteLine($"[SIMULACAO] Voo {voo.Id.ToString()[..4]}... INICIADO. {voo.DroneAlocado.Nome} partiu da Base.");
                }
                // Se não estiver em Carregando, o Gerenciador de Frota fez algo errado, ou o drone já está ocupado.
            }
        }

        public void ProcessarCicloDeSimulacao(double tempoDecorridoMinutos)
        {
            // O algoritmo de simulação real é complexo, envolve calcular a posição exata, bateria, etc.
            // Para o básico, vamos simular a conclusão do voo após um certo tempo ou distância percorrida.

            Console.WriteLine($"\n--- Processando Simulação. Tempo decorrido: {tempoDecorridoMinutos} minutos ---");

            var voosConcluidos = new List<Voo>();

            // Itera sobre todos os voos em andamento
            foreach (var voo in _voosEmAndamento)
            {
                var drone = voo.DroneAlocado;

                // Simulação simplificada de "tempo de voo" baseado na distância total
                // Assumimos que a velocidade média é 1 km/min (ou seja, Distancia = Tempo).
                double tempoNecessarioParaConcluir = voo.DistanciaTotalRotaKm;

                // Se o tempo decorrido total da simulação for maior ou igual ao tempo total de voo...
                // (Aqui teríamos um controle de progresso mais detalhado, mas para o desafio, simplificamos)
                if (tempoNecessarioParaConcluir < tempoDecorridoMinutos * 10) // Fator de aceleração da simulação
                {
                    voosConcluidos.Add(voo);

                    // Atualiza o drone para o estado final
                    drone.Status = DroneStatus.Idle;
                    drone.LocalizacaoAtual = Ponto.Base; // Assumimos que ele voltou para a base

                    Console.WriteLine($"[CONCLUIDO] {drone.Nome} completou o Voo {voo.Id.ToString()[..4]}... Total de pacotes: {voo.Pacotes.Count}.");
                    Console.WriteLine($"[STATUS] {drone.Nome} está agora: {drone.Status}");
                }
                else
                {
                    // No mundo real, aqui você atualizaria a LocalizacaoAtual e a Bateria.
                    // Por agora, apenas indicamos que ele está em voo.
                    Console.WriteLine($"[ANDAMENTO] {drone.Nome} em voo. Resta aprox. {tempoNecessarioParaConcluir - (tempoDecorridoMinutos * 10)} km para o destino.");
                }
            }

            // Remove os voos concluídos
            voosConcluidos.ForEach(v => _voosEmAndamento.Remove(v));
        }

        public List<Voo> GetVoosEmAndamento()
        {
            return _voosEmAndamento;
        }
    }
}