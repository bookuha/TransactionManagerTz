using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using GeoTimeZone;
using TransactionManager.Data.Model;

namespace TransactionManager.Logic.CsvHelpers;

public class LocationToTimeZoneConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow? row, MemberMapData? memberMapData)
    {
        if (text is null)
        {
            return base.ConvertFromString(text, row, memberMapData);
        }
        var location =
            new LocationConverter().ConvertFromString(text, null, null) as Location;
        if (location is null) return base.ConvertFromString(text, row, memberMapData);
        
        var tzIanaId = TimeZoneLookup.GetTimeZone(location.Latitude, location.Longitude).Result;
        return tzIanaId;

    }
}