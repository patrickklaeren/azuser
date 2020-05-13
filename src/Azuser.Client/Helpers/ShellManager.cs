using Azuser.Client.Framework;

namespace Azuser.Client.Helpers
{
    public interface IShellManager
    {
        void SetLoadingData(bool isLoadingData);
    }

    public class ShellManager : IShellManager
    {
        private readonly IMessengerService _messengerService;

        public ShellManager(IMessengerService messengerService)
        {
            _messengerService = messengerService;
        }

        public void SetLoadingData(bool isLoadingData)
        {
            _messengerService.Send(new LoadingDataMessage(isLoadingData));
        }
    }

    public class LoadingDataMessage
    {
        public LoadingDataMessage(bool isLoadingData)
        {
            IsLoadingData = isLoadingData;
        }

        public bool IsLoadingData { get; }
    }
}
