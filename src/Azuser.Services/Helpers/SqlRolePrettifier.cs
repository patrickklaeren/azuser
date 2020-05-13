using System;

namespace Azuser.Services.Helpers
{
    internal static class SqlRolePrettifier
    {
        internal static string Prettify(string rawSqlRoleName)
        {
            switch (rawSqlRoleName)
            {
                case SqlConstants.OWNER:
                    return "Owner";
                case SqlConstants.ACCESS_ADMIN:
                    return "Access Admin";
                case SqlConstants.SECURITY_ADMIN:
                    return "Security Admin";
                case SqlConstants.DDL_ADMIN:
                    return "DDL Admin";
                case SqlConstants.BACKUP_OPERATOR:
                    return "Backup Operator";
                case SqlConstants.DATA_READER:
                    return "Data Reader";
                case SqlConstants.DATA_WRITER:
                    return "Data Writer";
                case SqlConstants.DENY_DATA_READER:
                    return "Deny Data Reader";
                case SqlConstants.DENY_DATA_WRITER:
                    return "Deny Data Writer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(rawSqlRoleName));
            }
        }
    }
}
