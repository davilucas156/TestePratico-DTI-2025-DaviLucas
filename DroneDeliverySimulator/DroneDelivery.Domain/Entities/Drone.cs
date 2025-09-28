using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using System;

namespace DroneDelivery.Domain.Entities
{
    public class Drone
    {
        public const double ALCANCE_MAXIMO_ROTA_KM = 141.42;

        public Guid Id { get; } = Guid.NewGuid();
        public string Nome { get; set; }

        // Regras Básicas e de Simulação
        public double CapacidadeMaxKg { get; }
        public double AlcanceMaxKm { get; } 
        public double VelocidadeMediaKmh { get; set; }
        public DroneStatus Status { get; set; } = DroneStatus.Idle;
        public Ponto LocalizacaoAtual { get; set; } = Ponto.Base;

        public double NivelBateriaPercentual { get; set; }
        public double ConsumoKmPorPercentual { get; } = 2.0; 
        public int CiclosDeRecargaRestantes { get; set; } = 0;

        public Drone(string nome, double capacidadeMaxKg, double velocidade)
        {
            Nome = nome;
            CapacidadeMaxKg = capacidadeMaxKg;
            VelocidadeMediaKmh = velocidade;

            AlcanceMaxKm = ALCANCE_MAXIMO_ROTA_KM;
            NivelBateriaPercentual = 100.0; 

            if (capacidadeMaxKg <= 0)
            {
                throw new ArgumentException("Capacidade deve ser um valor positivo.");
            }
        }

        public double GetAlcanceEfetivoKm(double pesoTotalCarga)
        {
            double percentual;

            if (pesoTotalCarga <= 3.0)
            {
                percentual = 1.0; 
            }
            else if (pesoTotalCarga <= 6.0)
            {
                percentual = 0.80;
            }
            else if (pesoTotalCarga <= 10.0)
            {
                percentual = 0.70;
            }
            else
            {
                percentual = 0.60;
            }

            return ALCANCE_MAXIMO_ROTA_KM * percentual;
        }

        public override string ToString()
        {
            return $"{Nome} (ID: {Id.ToString()[..4]}...) | Status: {Status} | Capacidade: {CapacidadeMaxKg}kg | Bateria: {NivelBateriaPercentual:F1}%";
        }
    }
}