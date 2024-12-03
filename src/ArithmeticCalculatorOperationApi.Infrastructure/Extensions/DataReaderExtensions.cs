using System.Data.Common;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Extensions
{
    public static class DataReaderExtensions
    {
        public static bool HasColumn(this DbDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
