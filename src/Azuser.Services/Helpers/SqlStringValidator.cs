using System;
using System.Linq;

namespace Azuser.Services.Helpers
{
    internal static class SqlStringValidator
    {
        internal static bool HasInvalidSymbols(string input)
        {
           var symbols = new [] { '\'', '(', ')', '/', '#', '%', '&', '\\', ':', ';', '<', '>', '=', '[', ']', '?', '`', '|' };

            return input.Any(x => symbols.Contains(x));
        }

        internal static bool HasInjectionCommand(string input)
        {
            var injectionCommands = new[] { "select", "drop", "--", "insert", "delete", "xp_" };

            foreach (var injectionCommand in injectionCommands)
            {
                if (input.IndexOf(injectionCommand, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsValidRole(string rawSqlRoleName)
        {
            switch (rawSqlRoleName)
            {
                case SqlConstants.OWNER:
                case SqlConstants.ACCESS_ADMIN:
                case SqlConstants.SECURITY_ADMIN:
                case SqlConstants.DDL_ADMIN:
                case SqlConstants.BACKUP_OPERATOR:
                case SqlConstants.DATA_READER:
                case SqlConstants.DATA_WRITER:
                case SqlConstants.DENY_DATA_READER:
                case SqlConstants.DENY_DATA_WRITER:
                    return true;
                default:
                    return false;
            }
        }
    }
}
