using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Entities
{

    public class Pedido
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Ponto LocalizacaoCliente { get; }
        public double Peso { get; }
        public Prioridade Prioridade { get; }
        public DateTime HoraRecebimento { get; } = DateTime.Now;

   
        public Pedido(Ponto localizacao, double peso, Prioridade prioridade)
        {
            if (peso <= 0)
            {
                throw new ArgumentException("O peso do pacote deve ser maior que zero.", nameof(peso));
            }

            LocalizacaoCliente = localizacao;
            Peso = peso;
            Prioridade = prioridade;
        }

        public override string ToString()
        {
            return $"[ID: {Id.ToString()[..4]}...] | Peso: {Peso}kg | Local: {LocalizacaoCliente} | Prioridade: {Prioridade}";
        }
    }
}