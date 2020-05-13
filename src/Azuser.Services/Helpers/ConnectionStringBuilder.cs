using System.Data.SqlClient;

namespace Azuser.Services.Helpers
{
    internal static class ConnectionStringBuilder
    {
        internal static string FromCredentials(string serverAddress, string username, string password, string startupDatabase = "master")
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverAddress,
                UserID = username,
                Password = password,
                InitialCatalog = startupDatabase,
            };

            return builder.ConnectionString;
        }
    }
}