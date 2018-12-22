namespace Azuser.Client.DatabaseScopes
{
    public class ConnectionScope
    {
        public ConnectionScope(string serverAddress, string username, string password)
        {
            ServerAddress = serverAddress;
            Username = username;
            Password = password;
        }

        public string ServerAddress { get; }
        public string Username { get; }
        public string Password { get; }
    }
}