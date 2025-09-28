using DroneDelivery.Domain.Entities;
using System.Collections.Generic;

namespace DroneDelivery.Domain.Services
{
    public interface IGerenciadorDeFrota
    {
        List<Voo> AlocarPedidos(List<Pedido> pedidosPendentes);  
        IEnumerable<Drone> GetDrones();
    }
}   