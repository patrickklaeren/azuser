using System.Data;
using System.Data.SqlClient;

namespace Azuser.Services.Helpers
{
    internal static class SqlParameterCollectionExtensions
    {
        internal static SqlParameterCollection Add(this SqlParameterCollection collection, string parameterName, string value)
        {
            var parameter = new SqlParameter
            {
                ParameterName = parameterName,
                Value = value
            };

            collection.Add(parameter);

            return collection;
        }
    }
}