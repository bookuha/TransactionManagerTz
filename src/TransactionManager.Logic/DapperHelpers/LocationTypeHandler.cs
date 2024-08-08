using System.Data;
using Dapper;
using TransactionManager.Data.Model;
using TransactionManager.Logic.CsvHelpers;

namespace TransactionManager.Logic.DapperHelpers;

public class LocationTypeHandler : SqlMapper.TypeHandler<Location>
{
    public override void SetValue(IDbDataParameter parameter, Location? value)
    {
        parameter.Value = value is null ? DBNull.Value : value.ToString();
    }

    public override Location? Parse(object value)
    {
        if (value is null or DBNull) return default;

        if (value is not string s) return default;

        return new LocationConverter().ConvertFromString(s, null, null) as Location;
    }
}