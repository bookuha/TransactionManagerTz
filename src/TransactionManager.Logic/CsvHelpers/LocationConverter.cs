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

        try
        {
            return Location.FromString(text);
        }
        catch (Exception)
        {
            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}