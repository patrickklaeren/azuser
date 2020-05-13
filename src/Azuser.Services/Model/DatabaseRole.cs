using Azuser.Services.Helpers;

namespace Azuser.Services.Model
{
    public sealed class DatabaseRole
    {
        internal DatabaseRole(string rawSqlRoleName)
        {
            RawSqlRoleName = rawSqlRoleName;
            Name = SqlRolePrettifier.Prettify(rawSqlRoleName);
        }

        public string RawSqlRoleName { get; }
        public string Name { get; }
        public bool Value { get; internal set; }
    }
}