namespace DroneDelivery.Domain.Models
{
    
    public struct Ponto
    {
        public double X { get; }
        public double Y { get; }

        public Ponto(double x, double y)
        {
            X = x;
            Y = y;
        }
        public static Ponto Base => new Ponto(0, 0);

    
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}