using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;

namespace DroneDelivery.Domain.Entities
{
    /// <summary>
    /// Representa um pacote a ser entregue.
    /// É uma entidade central no sistema de simulação.
    /// </summary>
    public class Pedido
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Ponto LocalizacaoCliente { get; }
        public double Peso { get; }
        public Prioridade Prioridade { get; }
        public DateTime HoraRecebimento { get; } = DateTime.Now;

        /// <summary>
        /// Construtor para criar um novo pedido.
        /// Valida as regras de peso (não pode ser zero ou negativo).
        /// </summary>
        /// <param name="localizacao">Coordenadas de entrega do cliente.</param>
        /// <param name="peso">Peso do pacote em kg.</param>
        /// <param name="prioridade">Nível de prioridade da entrega.</param>
        public Pedido(Ponto localizacao, double peso, Prioridade prioridade)
        {
            // Validação de Erro Mandatória: Tratamento de entradas inválidas.
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