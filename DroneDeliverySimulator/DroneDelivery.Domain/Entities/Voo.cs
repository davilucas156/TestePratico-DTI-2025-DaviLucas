using DroneDelivery.Domain.Models;
using DroneDelivery.Domain.Services;
using System.Linq;
using System;

namespace DroneDelivery.Domain.Entities
{
   
    public class Voo
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Drone DroneAlocado { get; }
        public List<Pedido> Pacotes { get; } = new List<Pedido>();

        public double PesoTotalCarga { get; private set; }
        public double DistanciaTotalRotaKm { get; private set; }

        public double TempoTotalEstimadoMinutos { get; private set; }

        public double TempoDecorridoNoVooMinutos { get; set; }


        public List<Ponto> RotaPlanejada { get; private set; } = new List<Ponto>();

        public Voo(Drone drone)
        {
            DroneAlocado = drone;
            RotaPlanejada.Add(Ponto.Base);
        }

        public void AdicionarPacote(Pedido pedido, ICalculadoraDistancia calculadora)
        {
            // Validação de erro essencial:
            if (PesoTotalCarga + pedido.Peso > DroneAlocado.CapacidadeMaxKg)
            {
                throw new InvalidOperationException($"O pacote excede a capacidade máxima do {DroneAlocado.Nome}.");
            }

            Pacotes.Add(pedido);
            PesoTotalCarga += pedido.Peso;

            RotaPlanejada.Add(pedido.LocalizacaoCliente);

            RecalcularDistanciaTotal(calculadora);

            if (DistanciaTotalRotaKm > DroneAlocado.AlcanceMaxKm)
            {
                throw new InvalidOperationException($"A rota com este pacote excede o alcance máximo ({DroneAlocado.AlcanceMaxKm}km).");
            }
            RecalcularTempoTotal();

        }

        internal void RecalcularDistanciaTotal(ICalculadoraDistancia calculadora)
        {
            DistanciaTotalRotaKm = 0;
            Ponto pontoAnterior = Ponto.Base;

            var rotaCompleta = new List<Ponto>(RotaPlanejada);

            if (rotaCompleta.Last().X != Ponto.Base.X || rotaCompleta.Last().Y != Ponto.Base.Y)
            {
                rotaCompleta.Add(Ponto.Base);
            }

            for (int i = 1; i < rotaCompleta.Count; i++)
            {
                DistanciaTotalRotaKm += calculadora.CalcularDistancia(rotaCompleta[i - 1], rotaCompleta[i]);
            }
        }

        public void RecalcularTempoTotal()
        {
            if (DroneAlocado.VelocidadeMediaKmh > 0)
            {
                TempoTotalEstimadoMinutos = (DistanciaTotalRotaKm / DroneAlocado.VelocidadeMediaKmh) * 10.0;
            }
            else
            {
                TempoTotalEstimadoMinutos = 99999.0;
            }
        }

        
        internal void AdicionarPacoteSemValidacao(Pedido pedido)
        {
            Pacotes.Add(pedido);
            PesoTotalCarga += pedido.Peso;
            RotaPlanejada.Add(pedido.LocalizacaoCliente);
        }

        public void AdicionarPacoteParaTeste(Pedido pedido, ICalculadoraDistancia calculadora)
        {
            // Apenas adiciona os dados
            Pacotes.Add(pedido);
            PesoTotalCarga += pedido.Peso;
            RotaPlanejada.Add(pedido.LocalizacaoCliente);

            // Calcula a nova rota
            RecalcularDistanciaTotal(calculadora);

        }
    }
}