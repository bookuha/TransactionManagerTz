using CsvHelper.Configuration;
using TransactionManager.Data.Model;

namespace TransactionManager.Logic.CsvHelpers;

public sealed class TransactionCsvMap : ClassMap<Transaction>
{
    public TransactionCsvMap()
    {
        Map(m => m.Id).Name("transaction_id");
        Map(m => m.Name).Name("name");
        Map(m => m.Email).Name("email");
        Map(m => m.Amount).Name("amount").TypeConverter<DollarCurrencyConverter>();
        Map(m => m.Date).Name("transaction_date").TypeConverter<DateTimeToUtcConverter>();
        Map(m => m.IanaTimeZone).Name("client_location").TypeConverter<LocationToTimeZoneConverter>();
        Map(m => m.ClientLocation).Name("client_location").TypeConverter<LocationConverter>();
    }
}