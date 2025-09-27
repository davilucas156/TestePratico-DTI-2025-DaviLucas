namespace DroneDelivery.Domain.Enums
{
    /// <summary>
    /// Representa os diferentes estados do drone no ciclo de simulação.
    /// </summary>
    public enum DroneStatus
    {
        Idle,         // Parado e disponível
        Carregando,   // Montando a carga de pacotes
        EmVoo,        // Em trânsito para a primeira entrega
        Entregando,   // Executando a entrega em um local
        Retornando,   // Voltando para a base após a última entrega
        EmRecarga     // Carregando a bateria (se a simulação incluir tempo de recarga)
    }
}