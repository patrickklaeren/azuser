using System;
using Azuser.Services;

namespace Azuser.Client.Views.Explorer
{
    public class ServerLoginViewModel
    {
        public ServerLoginViewModel(Services.Model.ServerLogin serverLogin)
        {
            Name = serverLogin.Name;
            PrincipalId = serverLogin.PrincipalId;
            IsDisabled = serverLogin.IsDisabled;
            Created = serverLogin.Created;
            LastModified = serverLogin.LastModified;
            DefaultDatabase = serverLogin.DefaultDatabase;
        }

        public string Name { get; set; }
        public int PrincipalId { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string DefaultDatabase { get; set; }
    }
}