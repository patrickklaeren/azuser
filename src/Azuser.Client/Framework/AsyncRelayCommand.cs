using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Azuser.Client.Framework
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _action;

        private bool _canExecute = true;

        public AsyncRelayCommand(Func<Task> task)
        {
            _action = task;
        }

        public bool CanExecute(object parameter) => _canExecute;

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        public async void Execute(object parameter)
        {
            try
            {
                if (CanExecute(null) == false)
                    return;

                SetCanExecute(false);

                await _action();
            }
            finally
            {
                SetCanExecute(true);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
