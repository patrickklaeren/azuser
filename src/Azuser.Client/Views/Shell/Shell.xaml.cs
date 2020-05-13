namespace Azuser.Client.Views.Shell
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        public Shell(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += async (sender, args) => await viewModel.InitializeAsync();
        }
    }
}
