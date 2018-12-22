using System;

namespace Azuser.Client.Helpers
{
    public interface IMessengerService
    {
        void Send<TMessage>(TMessage message);
        void Register<TMessage>(object receipient, Action<TMessage> action);
        void UnregisterAllMessages(object context);
    }

    public class MessengerService : IMessengerService
    {
        public void Send<TMessage>(TMessage message)
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(message);
        }

        public void Register<TMessage>(object receipient, Action<TMessage> action)
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register(receipient, action);
        }

        public void UnregisterAllMessages(object context)
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Unregister(context);
        }
    }
}
