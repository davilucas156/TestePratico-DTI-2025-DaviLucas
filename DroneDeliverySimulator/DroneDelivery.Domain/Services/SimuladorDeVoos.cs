using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Gerencia o ciclo de vida dos voos e a transição de estado dos drones, rastreando o progresso do tempo.
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

                    // 3. Reseta o tempo decorrido, caso o objeto Voo esteja sendo reusado (boa prática)
                    voo.TempoDecorridoNoVooMinutos = 0.0;

                    Console.WriteLine($"[SIMULACAO] Voo {voo.Id.ToString()[..4]}... INICIADO. {voo.DroneAlocado.Nome} partiu da Base.");
                }
                // Se não estiver em Carregando, o Gerenciador de Frota fez algo errado, ou o drone já está ocupado.
            }
        }

        /// <summary>
        /// Avança a simulação pelo tempo especificado, atualizando o progresso dos voos.
        /// </summary>
        /// <param name="tempoDecorridoMinutos">A unidade de tempo a ser avançada (ex: 1.0, 5.0).</param>
        // Dentro da classe SimuladorDeVoos.cs

        public void ProcessarCicloDeSimulacao(double tempoDecorridoMinutos)
        {
            // O algoritmo de simulação real é complexo, envolve calcular a posição exata, bateria, etc.
            // Para o básico, vamos simular a conclusão do voo após um certo tempo ou distância percorrida.

            Console.WriteLine($"\n--- Processando Simulação. Tempo decorrido: {tempoDecorridoMinutos} minuto(s) ---");

            var voosConcluidos = new List<Voo>();

            // Itera sobre todos os voos em andamento
            foreach (var voo in _voosEmAndamento)
            {
                var drone = voo.DroneAlocado;

                // 1. Acumula o tempo que passou neste ciclo
                voo.TempoDecorridoNoVooMinutos += tempoDecorridoMinutos;

                // 2. Calcula o tempo restante
                var tempoRestanteMinutos = voo.TempoTotalEstimadoMinutos - voo.TempoDecorridoNoVooMinutos;

                // Checa se o voo foi concluído
                if (tempoRestanteMinutos <= 0)
                {
                    // O Voo terminou!
                    voosConcluidos.Add(voo);

                    // Atualiza o drone para o estado final
                    drone.Status = DroneStatus.Idle;
                    drone.LocalizacaoAtual = Ponto.Base; // VOLTOU para a base

                    // Log de conclusão
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CONCLUIDO] {drone.Nome} completou o Voo {voo.Id.ToString()[..4]}... Total de pacotes: {voo.Pacotes.Count}.");
                    Console.WriteLine($"[STATUS] {drone.Nome} está agora: {drone.Status}");
                    Console.ResetColor();
                }
                else
                {
                    // O voo ainda está em andamento. ATUALIZAR POSIÇÃO.

                    // --- NOVO CÁLCULO DE POSIÇÃO (INTERPOLAÇÃO LINEAR) ---

                    // Premissa: Simulação de rota simplificada Base (0,0) -> Cliente (X,Y) -> Base (0,0).

                    // 1. Ponto de Destino (Usamos a localização do primeiro pacote como destino)
                    var destino = voo.Pacotes.First().LocalizacaoCliente;

                    // 2. Tempo de ida (Metade do tempo total)
                    var tempoDeIdaMinutos = voo.TempoTotalEstimadoMinutos / 2.0;

                    double progresso;

                    if (voo.TempoDecorridoNoVooMinutos <= tempoDeIdaMinutos)
                    {
                        // IDA (Base para Cliente)
                        progresso = voo.TempoDecorridoNoVooMinutos / tempoDeIdaMinutos;
                    }
                    else
                    {
                        // VOLTA (Cliente para Base)
                        var tempoDecorridoNaVolta = voo.TempoDecorridoNoVooMinutos - tempoDeIdaMinutos;

                        // Calcula o tempo que falta para chegar à base
                        var tempoRestanteVolta = tempoDeIdaMinutos - tempoDecorridoNaVolta;

                        // O progresso é o tempo que falta para chegar na base, dividido pelo tempo total da volta.
                        progresso = tempoRestanteVolta / tempoDeIdaMinutos;
                    }

                    // Garante que o progresso esteja entre 0 e 1 (por segurança)
                    progresso = Math.Max(0, Math.Min(1, progresso));

                    // Interpolação Linear: (Posição Inicial * (1-Progresso) + Posição Final * Progresso)
                    // Como a Base (inicial e final) é (0,0), a fórmula simplifica para:
                    var xAtual = destino.X * progresso;
                    var yAtual = destino.Y * progresso;

                    drone.LocalizacaoAtual = new Ponto(xAtual, yAtual);

                    // --- FIM DO CÁLCULO DE POSIÇÃO ---

                    // Para feedback visual claro:
                    var distanciaRestanteKm = (tempoRestanteMinutos / 60.0) * drone.VelocidadeMediaKmh;

                    Console.WriteLine($"[ANDAMENTO] {drone.Nome} em voo. Resta aprox. {distanciaRestanteKm:F1} km ({tempoRestanteMinutos:F1} min). Posição: ({xAtual:F1}, {yAtual:F1}).");
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