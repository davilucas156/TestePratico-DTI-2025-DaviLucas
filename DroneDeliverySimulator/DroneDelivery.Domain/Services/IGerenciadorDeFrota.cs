using DroneDelivery.Domain.Entities;
using System.Collections.Generic;

namespace DroneDelivery.Domain.Services
{
    /// <summary>
    /// Contrato para o serviço que contém a lógica de alocação de pedidos e otimização de rotas.
    /// </summary>
    public interface IGerenciadorDeFrota
    {
        /// <summary>
        /// Recebe a lista de pedidos pendentes e tenta alocá-los nos drones disponíveis,
        /// gerando uma lista de voos otimizados.
        /// </summary>
        /// <param name="pedidosPendentes">Lista de pedidos que aguardam alocação.</param>
        /// <returns>Uma lista de Voos planejados.</returns>
        List<Voo> AlocarPedidos(List<Pedido> pedidosPendentes);

        /// <summary>
        /// Obtém a lista atual de drones gerenciados.
        /// </summary>
        IEnumerable<Drone> GetDrones();
    }
}   