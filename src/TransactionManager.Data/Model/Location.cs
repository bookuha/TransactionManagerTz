namespace TransactionManager.Data.Model;

public readonly record struct Location(double Latitude, double Longitude)
{
    public override string ToString()
    {
        return Latitude + ", " + Longitude;
    }
}
