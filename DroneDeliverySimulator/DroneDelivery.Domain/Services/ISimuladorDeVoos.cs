using DroneDelivery.Domain.Entities;
using System.Collections.Generic;


namespace DroneDelivery.Domain.Services
{
    
    public interface ISimuladorDeVoos
    {
       
        void IniciarVoos(List<Voo> voos);
        void ProcessarCicloDeSimulacao(double tempoDecorridoMinutos);

      
        List<Voo> GetVoosEmAndamento();
    }
}