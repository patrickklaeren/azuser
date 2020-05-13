using System.Windows;

namespace Azuser.Client.Helpers
{
    public interface IMessageBoxHelper
    {
        void ShowError(string message, string title = "An error has occurred");
        void ShowWarning(string message, string title = "Warning");
        MessageBoxResult ShowDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
    }

    public class MessageBoxHelper : IMessageBoxHelper
    {
        public void ShowError(string message, string title = "An error has occurred")
        {
            ShowDialog(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            ShowDialog(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public MessageBoxResult ShowDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }
    }
}
