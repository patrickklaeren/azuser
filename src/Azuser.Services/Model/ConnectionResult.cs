namespace Azuser.Services.Model
{
    public sealed class ConnectionResult
    {
        internal ConnectionResult(bool isSuccessful)
        {
            IsSuccessful = isSuccessful;
        }

        internal ConnectionResult(bool isSuccessful, string message)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }

        public bool IsSuccessful { get; }
        public string Message { get; }
    }
}