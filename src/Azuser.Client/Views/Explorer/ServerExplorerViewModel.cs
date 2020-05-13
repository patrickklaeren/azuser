using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Framework;
using Azuser.Client.Helpers;
using Azuser.Client.Views.User;
using Azuser.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
 
namespace Azuser.Client.Views.Explorer
{
    public class ServerExplorerViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IShellManager _shellManager;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private List<DatabaseViewModel> _databaseItemsSource;
        private List<ServerLoginViewModel> _serverLoginItemsSource;
        private ConnectionScope _currentConnectionScope;
        private ViewModelBase _overlayViewModel;
        private ServerLoginViewModel _selectedLogin;

        public ServerExplorerViewModel(IDatabaseService databaseService, IShellManager shellManager, 
            IMessageBoxHelper messageBoxHelper, IServiceScopeFactory serviceScopeFactory)
        {
            _databaseService = databaseService;
            _shellManager = shellManager;
            _messageBoxHelper = messageBoxHelper;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public bool ShowOverlay => OverlayViewModel != null;

        public ViewModelBase OverlayViewModel
        {
            get => _overlayViewModel;
            set
            {
                _overlayViewModel?.Cleanup();

                if (Set(ref _overlayViewModel, value))
                {
                    RaisePropertyChanged(() => ShowOverlay);
                }
            }
        }

        public List<DatabaseViewModel> DatabaseItemsSource
        {
            get => _databaseItemsSource;
            set => Set(ref _databaseItemsSource, value);
        }

        public List<ServerLoginViewModel> ServerLoginItemsSource
        {
            get => _serverLoginItemsSource;
            set => Set(ref _serverLoginItemsSource, value);
        }

        public ServerLoginViewModel SelectedLogin
        {
            get => _selectedLogin;
            set
            {
                if (Set(ref _selectedLogin, value))
                {
                    RaisePropertyChanged(() => EnableDeleteLogin);
                }
            }
        }

        public bool EnableDeleteLogin => SelectedLogin != null;

        public async Task InitializeAsync(ConnectionScope connectionScope)
        {
            try
            {
                _currentConnectionScope = connectionScope;

                _shellManager.SetLoadingData(true);

                var serverAddress = connectionScope.ServerAddress;
                var username = connectionScope.Username;
                var password = connectionScope.Password;

                var databases = await Task.Run(() => _databaseService.GetDatabasesForServer(serverAddress, username, password));

                var logins = await Task.Run(() => _databaseService.GetLoginsForServer(serverAddress, username, password));

                DatabaseItemsSource = databases.Select(x => new DatabaseViewModel(x)).ToList();

                ServerLoginItemsSource = logins.Select(x => new ServerLoginViewModel(x)).ToList();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed loading databases and/or users for connection");

                _messageBoxHelper.ShowError("Failed getting information for connection, please try again. If the issue persists, check the log.");
            }
            finally
            {
                _shellManager.SetLoadingData(false);
            }
        }

        public ICommand AddLoginCommand => new AsyncRelayCommand(AddLogin);

        private async Task AddLogin()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            
            var viewModel = scope.ServiceProvider.GetRequiredService<LoginDetailsViewModel>();

            OverlayViewModel = viewModel;

            var hasAddedLogin = await viewModel.WaitForLoginDetails(_currentConnectionScope);

            OverlayViewModel = null;

            if (!hasAddedLogin)
                return;

            await InitializeAsync(_currentConnectionScope);
        }

        public ICommand EditLoginCommand => new AsyncRelayCommand(EditLogin);

        private async Task EditLogin()
        {
            if (SelectedLogin == null)
                return;
            
            using var scope = _serviceScopeFactory.CreateScope();

            var viewModel = scope.ServiceProvider.GetRequiredService<LoginDetailsViewModel>();

            OverlayViewModel = viewModel;

            await viewModel.WaitForExistingLoginDetails(SelectedLogin.Name, _currentConnectionScope);

            OverlayViewModel = null;
        }

        public ICommand DeleteLoginCommand => new AsyncRelayCommand(DeleteLogin);

        private async Task DeleteLogin()
        {
            if (SelectedLogin == null)
                return;

            if (SelectedLogin.Name == _currentConnectionScope.Username)
                return;

            var confirmation = _messageBoxHelper.ShowDialog(
                "This will permanently remove the login from the server and the associated user will no longer be able to access the server, are you sure you want to continue?",
                "Remove login", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                _shellManager.SetLoadingData(true);

                var result = await Task.Run(() => _databaseService.DeleteLogin(_currentConnectionScope.ServerAddress,
                    _currentConnectionScope.Username, _currentConnectionScope.Password, SelectedLogin.Name));

                if (!result.IsSuccessful)
                {
                    Log.Error("Failed deleting login, with reason: {resultMessage}", result.Message);
                    _messageBoxHelper.ShowError($"Failed removing login: {result.Message}");
                    return;
                }

                await InitializeAsync(_currentConnectionScope);

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed removing login");

                _messageBoxHelper.ShowError("Something went wrong while removing the login, please try again. If the issue persists, check the log.");
            }
            finally
            {
                _shellManager.SetLoadingData(false);
            }
        }
    }
}
