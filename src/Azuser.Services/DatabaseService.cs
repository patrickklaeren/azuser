using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Azuser.Services.Helpers;
using Azuser.Services.Model;

namespace Azuser.Services
{
    public interface IDatabaseService
    {
        ConnectionResult ValidateConnection(string serverAddress, string username, string password);
        List<Database> GetDatabasesForServer(string serverAddress, string username, string password);
        List<ServerLogin> GetLoginsForServer(string serverAddress, string username, string password);

        List<DatabaseRole> GetRolesForUserInDatabase(string serverAddress, string username,
            string password, string catalog, string loginUsername);

        OperationResult AddLogin(string serverAddress, string username,
            string password, string loginUsername, string loginPassword);

        OperationResult DeleteLogin(string serverAddress, string username,
            string password, string loginUsername);

        OperationResult AddRoleForUser(string serverAddress, string username,
            string password, string catalog, string loginUsername, string roleName);

        OperationResult DeleteRoleForUser(string serverAddress, string username,
            string password, string catalog, string loginUsername, string roleName);
    }

    public sealed class DatabaseService : IDatabaseService
    {
        public ConnectionResult ValidateConnection(string serverAddress, string username, string password)
        {
            const string CONNECTION_COULD_NOT_BE_ESTABLISHED_CODE =
                "error: 40 - Could not open a connection to SQL Server";

            const string BLOCKED_BY_FIREWALL_CODE =
                "is not allowed to access the server. To enable access, use the Windows Azure Management Portal or run sp_set_firewall_rule";

            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password);

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }
            }
            catch (SqlException e)
            {
                if (e.Message.Contains(CONNECTION_COULD_NOT_BE_ESTABLISHED_CODE))
                {
                    return new ConnectionResult(false, "The target server is either not turned on, accepting connections or doesn't exist. No connection could be opened. Error: 40");
                }

                if (e.Message.Contains(BLOCKED_BY_FIREWALL_CODE))
                {
                    return new ConnectionResult(false, "The target server is refusing connections from this IP. To enable access use the Windows Azure Management Portal or run sp_set_firewall_rule");
                }

                throw;
            }

            return new ConnectionResult(true);
        }

        public List<Database> GetDatabasesForServer(string serverAddress, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var databases = new List<Database>();

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM sys.databases", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var databaseName = reader.GetString(0);
                        databases.Add(new Database(databaseName));
                    }
                }
            }

            return databases;
        }

        public List<ServerLogin> GetLoginsForServer(string serverAddress, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var logins = new List<ServerLogin>();

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM sys.sql_logins", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var userName = reader.GetString(0);
                        var principalId = reader.GetInt32(1);
                        var isDisabled = reader.GetBoolean(5);
                        var created = reader.GetDateTime(6);
                        var lastModified = reader.GetDateTime(7);
                        var defaultDatabase = reader.GetString(8);

                        logins.Add(new ServerLogin(userName, principalId, isDisabled, created, lastModified, defaultDatabase));
                    }
                }
            }

            return logins;
        }

        public OperationResult AddLogin(string serverAddress, string username,
            string password, string loginUsername, string loginPassword)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginUsername))
                throw new ArgumentNullException(nameof(loginUsername));

            if (string.IsNullOrWhiteSpace(loginPassword))
                throw new ArgumentNullException(nameof(loginPassword));

            var hasInvalidSymbols = SqlStringValidator.HasInvalidSymbols(loginUsername)
                                    || SqlStringValidator.HasInvalidSymbols(loginPassword);

            if (hasInvalidSymbols)
            {
                return new OperationResult(false, "The username or password contains illegal characters");
            }

            var isInjectionAttempt = SqlStringValidator.HasInjectionCommand(loginUsername)
                                     || SqlStringValidator.HasInjectionCommand(loginPassword);

            if (isInjectionAttempt)
            {
                return new OperationResult(false, "The username or password contains illegal characters");
            }

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand($"CREATE LOGIN [{loginUsername}] WITH PASSWORD = '{loginPassword}'", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            return new OperationResult(true);
        }

        public OperationResult DeleteLogin(string serverAddress, string username,
            string password, string loginUsername)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginUsername))
                throw new ArgumentNullException(nameof(loginUsername));

            var hasInvalidSymbols = SqlStringValidator.HasInvalidSymbols(loginUsername);

            if (hasInvalidSymbols)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var isInjectionAttempt = SqlStringValidator.HasInjectionCommand(loginUsername);

            if (isInjectionAttempt)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand($"DROP LOGIN [{loginUsername}]", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            return new OperationResult(true);
        }

        public List<DatabaseRole> GetRolesForUserInDatabase(string serverAddress, string username,
            string password, string catalog, string loginUsername)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(catalog))
                throw new ArgumentNullException(nameof(catalog));

            if (string.IsNullOrWhiteSpace(loginUsername))
                throw new ArgumentNullException(nameof(loginUsername));

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password, catalog);

            var roles = new List<DatabaseRole>
            {
                new DatabaseRole(SqlConstants.OWNER),
                new DatabaseRole(SqlConstants.ACCESS_ADMIN),
                new DatabaseRole(SqlConstants.SECURITY_ADMIN),
                new DatabaseRole(SqlConstants.DDL_ADMIN),
                new DatabaseRole(SqlConstants.BACKUP_OPERATOR),
                new DatabaseRole(SqlConstants.DATA_READER),
                new DatabaseRole(SqlConstants.DATA_WRITER),
                new DatabaseRole(SqlConstants.DENY_DATA_READER),
                new DatabaseRole(SqlConstants.DENY_DATA_WRITER),
            };

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                const string SQL =
                    "SELECT dbprincipals.name AS RoleName, dbuserprincipals.name AS Username FROM sys.database_role_members dbrolemembers "
                    + "INNER JOIN sys.database_principals dbprincipals ON dbprincipals.principal_id = dbrolemembers.role_principal_id "
                    + "INNER JOIN sys.database_principals dbuserprincipals ON dbuserprincipals.principal_id = dbrolemembers.member_principal_id "
                    + "WHERE dbuserprincipals.name = @loginUsername";

                using (var command = new SqlCommand(SQL, connection))
                {
                    command.Parameters.Add("@loginUsername", loginUsername);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ownedRole = reader.GetString(0);

                            var role = roles.SingleOrDefault(x => x.RawSqlRoleName == ownedRole);

                            if (role == null)
                                continue; // Unsupported by client

                            role.Value = true;
                        }
                    }
                }
            }

            return roles;
        }

        public OperationResult AddRoleForUser(string serverAddress, string username,
            string password, string catalog, string loginUsername, string roleName)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginUsername))
                throw new ArgumentNullException(nameof(loginUsername));

            var hasInvalidSymbols = SqlStringValidator.HasInvalidSymbols(loginUsername);

            if (hasInvalidSymbols)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var isInjectionAttempt = SqlStringValidator.HasInjectionCommand(loginUsername);

            if (isInjectionAttempt)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var isValidRole = SqlStringValidator.IsValidRole(roleName);

            if (!isValidRole)
            {
                return new OperationResult(false, "The given role does not exist in SQL");
            }

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password, catalog);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var hasCatalogUser = HasUserInDatabase(loginUsername, connection);

                if (!hasCatalogUser)
                {
                    // If the user doesn't have a mapping, we need to create a user
                    // in the target database prior to adding the role
                    using (var command = new SqlCommand($"CREATE USER [{loginUsername}] FOR LOGIN {loginUsername}", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var command = new SqlCommand("sp_addrolemember", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters
                        .Add("@membername", loginUsername)
                        .Add("@rolename", roleName);

                    using (var _ = command.ExecuteReader())
                    {
                        // Nothing much to do here
                    }
                }
            }

            return new OperationResult(true);
        }

        public OperationResult DeleteRoleForUser(string serverAddress, string username,
            string password, string catalog, string loginUsername, string roleName)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentNullException(nameof(serverAddress));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(loginUsername))
                throw new ArgumentNullException(nameof(loginUsername));

            var hasInvalidSymbols = SqlStringValidator.HasInvalidSymbols(loginUsername);

            if (hasInvalidSymbols)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var isInjectionAttempt = SqlStringValidator.HasInjectionCommand(loginUsername);

            if (isInjectionAttempt)
            {
                return new OperationResult(false, "The username contains illegal characters");
            }

            var isValidRole = SqlStringValidator.IsValidRole(roleName);

            if (!isValidRole)
            {
                return new OperationResult(false, "The given role does not exist in SQL");
            }

            var connectionString = ConnectionStringBuilder.FromCredentials(serverAddress, username, password, catalog);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var hasCatalogUser = HasUserInDatabase(loginUsername, connection);

                if (!hasCatalogUser)
                {
                    return new OperationResult(false, "The given user does not have this role in the target database");
                }

                using (var command = new SqlCommand("sp_droprolemember", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters
                        .Add("@membername", loginUsername)
                        .Add("@rolename", roleName);

                    using (var _ = command.ExecuteReader())
                    {
                        // Nothing much to do here
                    }
                }

                const string ROLES_IN_DATABASE_SQL =
                    "SELECT COUNT(*) FROM sys.database_role_members dbrolemembers "
                    + "INNER JOIN sys.database_principals dbprincipals ON dbprincipals.principal_id = dbrolemembers.role_principal_id "
                    + "INNER JOIN sys.database_principals dbuserprincipals ON dbuserprincipals.principal_id = dbrolemembers.member_principal_id "
                    + "WHERE dbuserprincipals.name = @loginUsername";

                bool hasOtherRolesInDatabase;

                using (var command = new SqlCommand(ROLES_IN_DATABASE_SQL, connection))
                {
                    command.Parameters
                        .Add("@loginUsername", loginUsername);

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        hasOtherRolesInDatabase = reader.GetInt32(0) >= 1;
                    }
                }

                if (!hasOtherRolesInDatabase)
                {
                    using (var command = new SqlCommand($"DROP USER [{loginUsername}]", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            return new OperationResult(true);
        }

        private static bool HasUserInDatabase(string loginUsername, SqlConnection connection)
        {
            using (var command = new SqlCommand("SELECT COUNT(*) FROM sys.database_principals WHERE [name] = @loginUsername", connection))
            {
                command.Parameters
                    .Add("@loginUsername", loginUsername);

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    return reader.GetInt32(0) == 1;
                }
            }
        }
    }
}