namespace Redakas.Graphs.Entities;

public class Position
{
    public double X { get; }
    public double Y { get; }

    public Position(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double EuclideanDistance(Position other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
