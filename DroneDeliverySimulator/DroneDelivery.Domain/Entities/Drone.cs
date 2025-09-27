using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Entities
{
    /// <summary>
    /// Representa a entidade Drone, com suas capacidades e estado atual.
    /// </summary>
    public class Drone
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Nome { get; set; }

        // Regras Básicas
        public double CapacidadeMaxKg { get; }
        public double AlcanceMaxKm { get; }
        public double VelocidadeMediaKmh { get; set; }

        // Estado Atual
        public DroneStatus Status { get; set; } = DroneStatus.Idle;
        public Ponto LocalizacaoAtual { get; set; } = Ponto.Base;



        /// <summary>
        /// Construtor para inicializar um drone com suas capacidades.
        /// </summary>
        public Drone(string nome, double capacidadeMaxKg, double alcanceMaxKm, double velocidade)
        {
            Nome = nome;
            CapacidadeMaxKg = capacidadeMaxKg;
            AlcanceMaxKm = alcanceMaxKm;
            VelocidadeMediaKmh = velocidade;
            // Validações de erro: Garantir que as capacidades são positivas
            if (capacidadeMaxKg <= 0 || alcanceMaxKm <= 0)
            {
                throw new ArgumentException("Capacidade e Alcance devem ser valores positivos.");
            }
        }

        // Método utilitário para visualização
        public override string ToString()
        {
            return $"{Nome} (ID: {Id.ToString()[..4]}...) | Status: {Status} | Capacidade: {CapacidadeMaxKg}kg | Alcance: {AlcanceMaxKm}km";
        }
    }
}