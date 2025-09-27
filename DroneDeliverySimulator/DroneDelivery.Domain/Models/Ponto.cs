namespace DroneDelivery.Domain.Models
{
    /// <summary>
    /// Representa uma coordenada 2D (X, Y) no mapa da cidade.
    /// Usamos um 'struct' pois é um tipo de valor pequeno e imutável.
    /// </summary>
    public struct Ponto
    {
        public double X { get; }
        public double Y { get; }

        public Ponto(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// A Base de Operações dos drones, geralmente (0, 0).
        /// </summary>
        public static Ponto Base => new Ponto(0, 0);

        /// <summary>
        /// Sobrescreve o ToString para facilitar a visualização no Terminal (ex: "(10, 5)").
        /// </summary>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}