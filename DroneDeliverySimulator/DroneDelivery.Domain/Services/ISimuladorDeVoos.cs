using DroneDelivery.Domain.Entities;
using System.Collections.Generic;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Define o contrato para simular a execução de voos e atualizar o estado dos drones.
    /// </summary>
    public interface ISimuladorDeVoos
    {
        /// <summary>
        /// Inicia a execução de uma lista de voos planejados, mudando o status dos drones de IDLE para EM VOO.
        /// </summary>
        /// <param name="voos">A lista de voos planejados pelo Gerenciador de Frota.</param>
        void IniciarVoos(List<Voo> voos);

        /// <summary>
        /// Simula um "tick" de tempo, atualizando o progresso e o estado dos drones em voo.
        /// Este é o coração da simulação orientada a eventos.
        /// </summary>
        /// <param name="tempoDecorridoMinutos">O tempo simulado que passou desde o último tick.</param>
        void ProcessarCicloDeSimulacao(double tempoDecorridoMinutos);

        /// <summary>
        /// Obtém todos os voos que ainda estão em andamento.
        /// </summary>
        List<Voo> GetVoosEmAndamento();
    }
}