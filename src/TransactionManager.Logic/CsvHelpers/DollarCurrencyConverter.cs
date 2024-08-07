using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace TransactionManager.Logic.CsvHelpers;

public class DollarCurrencyConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        var decimalConverter = new DecimalConverter();
        if (text is not null && text.StartsWith('$'))
        {
            return decimalConverter.ConvertFromString(text.Substring(1, text.Length - 1), row, memberMapData);
        }
        
        return decimalConverter.ConvertFromString(text, row, memberMapData);
    }        
}