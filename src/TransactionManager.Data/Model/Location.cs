using System.Text.RegularExpressions;

namespace TransactionManager.Data.Model;

public record Location(double Latitude, double Longitude)
{
    public override string ToString()
    {
        return Latitude + ", " + Longitude;
    }

    public static Location FromString(string str)
    {
        Regex locationRegex =
            new(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$");
        
        if (!locationRegex.IsMatch(str))
        {
            throw new ArgumentException("The location string is not in the correct format.");
        }
        
        var locationStringEntries = str.Split([", ", " "], StringSplitOptions.RemoveEmptyEntries);
        return new Location(Convert.ToDouble(locationStringEntries[0]), Convert.ToDouble(locationStringEntries[1]));
    }
}