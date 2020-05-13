using System;

namespace Azuser.Services.Model
{
    public sealed class ServerLogin
    {
        internal ServerLogin(string userName, int principalId, bool isDisabled, DateTime created, DateTime lastModified, string defaultDatabase)
        {
            Name = userName;
            PrincipalId = principalId;
            IsDisabled = isDisabled;
            Created = created;
            LastModified = lastModified;
            DefaultDatabase = defaultDatabase;
        }

        public string Name { get; }
        public int PrincipalId { get; }
        public bool IsDisabled { get; }
        public DateTime Created { get; }
        public DateTime LastModified { get; }
        public string DefaultDatabase { get; }
    }
}
