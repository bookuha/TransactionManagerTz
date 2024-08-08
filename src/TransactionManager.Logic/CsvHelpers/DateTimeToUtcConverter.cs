using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace TransactionManager.Logic.CsvHelpers;

public class DateTimeToUtcConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null) return base.ConvertFromString(text, row, memberMapData);

        var tzIanaId =
            new LocationToTimeZoneConverter().ConvertFromString(row.GetField("client_location"), null, null) as string;

        if (tzIanaId is null) return base.ConvertFromString(text, row, memberMapData);

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tzIanaId); // Note: Will still look up by IANA.

        DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var dateTime);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        DateTimeOffset utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);

        return utcDateTime;
    }
}