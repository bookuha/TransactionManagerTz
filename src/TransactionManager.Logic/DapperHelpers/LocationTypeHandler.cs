using System.Data;
using Dapper;
using TransactionManager.Data.Model;
using TransactionManager.Logic.CsvHelpers;

namespace TransactionManager.Logic.DapperHelpers;

public class LocationTypeHandler : SqlMapper.TypeHandler<Location>
{
    public override void SetValue(IDbDataParameter parameter, Location? value)
    {
        parameter.Value = value.ToString();
    }

    public override Location? Parse(object value)
    {
        if (value is string s)
        {
            return new LocationConverter().ConvertFromString(s, null, null) as Location;
        }

        return null;
    }
}