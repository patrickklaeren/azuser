namespace Azuser.Services.Model
{
    public sealed class OperationResult
    {
        internal OperationResult(bool successful)
        {
            IsSuccessful = successful;
        }

        internal OperationResult(bool successful, string message)
        {
            IsSuccessful = successful;
            Message = message;
        }

        public bool IsSuccessful { get; }
        public string Message { get; }
    }
}