namespace TransactionManager.Data.Model;

public record Location(double Latitude, double Longitude)
{
    public override string ToString()
    {
        return Latitude + ", " + Longitude;
    }
}