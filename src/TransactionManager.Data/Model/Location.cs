namespace TransactionManager.Data.Model;

public record Location(double Latitude, double Longitude)
{
    public override string ToString()
    {
        return Latitude + ", " + Longitude;
    }

    public static Location FromString(string str)
    {
        var locationStringEntries = str.Split([", ", " "], StringSplitOptions.RemoveEmptyEntries);

        return new Location(Convert.ToDouble(locationStringEntries[0]), Convert.ToDouble(locationStringEntries[1]));
    }
}