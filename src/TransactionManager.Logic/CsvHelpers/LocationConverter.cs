using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using TransactionManager.Data.Model;

namespace TransactionManager.Logic.CsvHelpers;

public class LocationConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow? row, MemberMapData? memberMapData)
    {
        if (text is null) return base.ConvertFromString(text, row, memberMapData);

        var locationStringEntries = text.Split([", ", " "], StringSplitOptions.RemoveEmptyEntries);

        if (locationStringEntries.Length == 2)
            return new Location(Convert.ToDouble(locationStringEntries[0]), Convert.ToDouble(locationStringEntries[1]));

        return base.ConvertFromString(text, row, memberMapData);
    }
}