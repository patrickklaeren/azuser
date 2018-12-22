namespace Azuser.Client.Views.Updater
{
    public class UpdaterViewModel : ViewModelBase
    {
        private int _currentProgress;

        public int MaxProgress => 100;

        public int CurrentProgress
        {
            get => _currentProgress;
            set => Set(ref _currentProgress, value);
        }
    }
}
