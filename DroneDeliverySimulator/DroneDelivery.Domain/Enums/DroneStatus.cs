namespace DroneDelivery.Domain.Enums
{
 
    public enum DroneStatus
    {
        Idle,         // Parado e disponível
        Carregando,   // Montando a carga de pacotes
        EmVoo,        // Em trânsito para a primeira entrega
        Entregando,   // Executando a entrega em um local
        Retornando,   // Voltando para a base após a última entrega
        EmRecarga     // Carregando a bateria 
    }
}