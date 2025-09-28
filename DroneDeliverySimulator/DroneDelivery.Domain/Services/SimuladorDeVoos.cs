using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DroneDelivery.Domain.Services
{
    public class SimuladorDeVoos : ISimuladorDeVoos
    {
        private readonly List<Voo> _voosEmAndamento = new List<Voo>();
        private readonly List<Drone> _frota; // Lista completa de drones

        private const int CICLOS_DE_RECARGA_TOTAL = 2;

        public SimuladorDeVoos(List<Drone> frota)
        {
            _frota = frota ?? throw new ArgumentNullException(nameof(frota), "A frota de drones deve ser fornecida ao simulador.");
        }

        public void IniciarVoos(List<Voo> voos)
        {
            if (voos == null || !voos.Any())
            {
                return;
            }

            foreach (var voo in voos)
            {
                if (voo.DroneAlocado.Status == DroneStatus.Carregando)
                {
                    voo.DroneAlocado.Status = DroneStatus.EmVoo;
                    _voosEmAndamento.Add(voo);
                    voo.TempoDecorridoNoVooMinutos = 0.0;

                    Console.WriteLine($"[SIMULACAO] Voo {voo.Id.ToString()[..4]}... INICIADO. {voo.DroneAlocado.Nome} partiu da Base.");
                }
            }
        }

        public void ProcessarCicloDeSimulacao(double tempoDecorridoMinutos)
        {
            Console.WriteLine($"\n--- Processando Simulação. Tempo decorrido: {tempoDecorridoMinutos} minuto(s) ---");

            var voosConcluidos = new List<Voo>();
            var origem = Ponto.Base;

            foreach (var drone in GetTodosDrones().Where(d => d.Status == DroneStatus.Carregando).ToList()) // ToList evita problemas de modificação da coleção durante a iteração
            {
                drone.CiclosDeRecargaRestantes--;

                if (drone.CiclosDeRecargaRestantes <= 0)
                {
                    drone.NivelBateriaPercentual = 100.0;
                    drone.Status = DroneStatus.Idle;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[RECARGA] {drone.Nome} completou a recarga e está {drone.Status}. Bateria: 100%.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"[RECARGA] {drone.Nome} carregando... {drone.CiclosDeRecargaRestantes} ciclo(s) restante(s).");
                }
            }


            foreach (var voo in _voosEmAndamento)
            {
                var drone = voo.DroneAlocado;

                voo.TempoDecorridoNoVooMinutos += tempoDecorridoMinutos;
                var tempoRestanteMinutos = voo.TempoTotalEstimadoMinutos - voo.TempoDecorridoNoVooMinutos;

                if (tempoRestanteMinutos <= 0)
                {
                    voosConcluidos.Add(voo);

                    drone.Status = DroneStatus.Idle;
                    drone.LocalizacaoAtual = origem;

                   
                    var consumoTotalPerc = voo.DistanciaTotalRotaKm / drone.ConsumoKmPorPercentual;
                    drone.NivelBateriaPercentual = Math.Max(0, drone.NivelBateriaPercentual - consumoTotalPerc);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CONCLUIDO] {drone.Nome} completou o Voo {voo.Id.ToString()[..4]}. Bateria: {drone.NivelBateriaPercentual:F1}%.");

                    // Condição para Iniciar Recarga Imediatamente
                    if (drone.NivelBateriaPercentual < 50.0 && drone.Status == DroneStatus.Idle)
                    {
                        drone.Status = DroneStatus.Carregando;
                        drone.CiclosDeRecargaRestantes = CICLOS_DE_RECARGA_TOTAL;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[ALERTA] {drone.Nome} com bateria baixa (<50%). Iniciando recarga ({CICLOS_DE_RECARGA_TOTAL} ciclos).");
                    }
                    Console.ResetColor();
                }
                else
                {
                    // Lógica de interpolação da posição
                    var destino = voo.Pacotes.First().LocalizacaoCliente;
                    var tempoDeIdaMinutos = voo.TempoTotalEstimadoMinutos / 2.0;

                    double progressoPerc;

                    if (voo.TempoDecorridoNoVooMinutos <= tempoDeIdaMinutos)
                    {
                        // IDA (Base para Cliente)
                        progressoPerc = voo.TempoDecorridoNoVooMinutos / tempoDeIdaMinutos;
                    }
                    else
                    {
                        // VOLTA (Cliente para Base)
                        var tempoDecorridoNaVolta = voo.TempoDecorridoNoVooMinutos - tempoDeIdaMinutos;
                        progressoPerc = 1.0 - (tempoDecorridoNaVolta / tempoDeIdaMinutos);
                    }

                    // Cálculo de posição
                    var xAtual = origem.X + (destino.X - origem.X) * progressoPerc;
                    var yAtual = origem.Y + (destino.Y - origem.Y) * progressoPerc;
                    drone.LocalizacaoAtual = new Ponto(xAtual, yAtual);

                    var distanciaRestanteKm = (tempoRestanteMinutos / 60.0) * drone.VelocidadeMediaKmh;
                    Console.WriteLine($"[ANDAMENTO] {drone.Nome} em voo. Posição: ({drone.LocalizacaoAtual.X:F1}, {drone.LocalizacaoAtual.Y:F1}). Bateria: {drone.NivelBateriaPercentual:F1}%.");
                }
            }

            voosConcluidos.ForEach(v => _voosEmAndamento.Remove(v));
        }

        // CORRIGIDO: Implementação real que retorna a frota.
        private IEnumerable<Drone> GetTodosDrones()
        {
            return _frota;
        }

        public List<Voo> GetVoosEmAndamento()
        {
            return _voosEmAndamento;
        }
    }
}