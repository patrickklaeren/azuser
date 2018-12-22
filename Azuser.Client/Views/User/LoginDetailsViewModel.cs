using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Framework;
using Azuser.Client.Framework.Resolver;
using Azuser.Client.Helpers;
using Azuser.Services;
using GalaSoft.MvvmLight.CommandWpf;
using Serilog;

namespace Azuser.Client.Views.User
{
    public class LoginDetailsViewModel : ViewModelBase
    {
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly IDatabaseService _databaseService;
        private readonly IShellManager _shellManager;
        private readonly IResolver _resolver;

        private string _username;
        private string _password;
        private string _confirmedPassword;

        private ConnectionScope _connectionScope;
        private readonly TaskCompletionSource<bool> _loginCompletionSource = new TaskCompletionSource<bool>();
        private bool _isNewUser = true;
        private bool _hasCreatedNewUserInSession;
        private ServerRolesViewModel _rolesViewModel;

        public LoginDetailsViewModel(IMessageBoxHelper messageBoxHelper, IDatabaseService databaseService,
            IShellManager shellManager, IResolver resolver)
        {
            _messageBoxHelper = messageBoxHelper;
            _databaseService = databaseService;
            _shellManager = shellManager;
            _resolver = resolver;
        }

        public ServerRolesViewModel RolesViewModel
        {
            get => _rolesViewModel;
            set => Set(ref _rolesViewModel, value);
        }

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        public string ConfirmedPassword
        {
            get => _confirmedPassword;
            set => Set(ref _confirmedPassword, value);
        }

        public bool IsNewUser
        {
            get => _isNewUser;
            set
            {
                if (Set(ref _isNewUser, value))
                {
                    RaisePropertyChanged(() => IsRolesSelected);
                }
            }
        }

        public bool IsRolesSelected => !IsNewUser;

        public Task<bool> WaitForLoginDetails(ConnectionScope connectionScope)
        {
            _connectionScope = connectionScope;

            return _loginCompletionSource.Task;
        }

        public async Task<bool> WaitForExistingLoginDetails(string username, ConnectionScope connectionScope)
        {
            _connectionScope = connectionScope;

            IsNewUser = false;
            Username = username;

            await LoadUserRoles();

            return await _loginCompletionSource.Task;
        }

        private async Task LoadUserRoles()
        {
            RolesViewModel = _resolver.Get<ServerRolesViewModel>();

            await RolesViewModel.InitializeAsync(Username, _connectionScope);
        }

        private string BuildValidation()
        {
            var builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(Username))
            {
                builder.AppendLine("- A valid server address needs to be entered");
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                builder.AppendLine("- A valid username needs to be entered");
            }

            if (string.IsNullOrWhiteSpace(ConfirmedPassword))
            {
                builder.AppendLine("- The password must be confirmed");
            }

            if (Password != ConfirmedPassword)
            {
                builder.AppendLine("- The confirmed password must match the previous password");
            }

            return builder.ToString();
        }

        public ICommand CancelCommand => new RelayCommand(Cancel);

        private void Cancel()
        {
            _loginCompletionSource.SetResult(_hasCreatedNewUserInSession);
        }

        public ICommand SaveCommand => new AsyncRelayCommand(Save);

        private async Task Save()
        {
            try
            {
                _shellManager.SetLoadingData(true);

                if (!IsNewUser)
                {
                    if (RolesViewModel != null)
                    {
                        await RolesViewModel.Save();
                    }

                    _loginCompletionSource.SetResult(_hasCreatedNewUserInSession);
                    return;
                }

                var validation = BuildValidation();

                if (!string.IsNullOrWhiteSpace(validation))
                {
                    _messageBoxHelper.ShowWarning($"Fix the following issues before the login can be created:\n\n{validation}");
                    return;
                }

                var result = await Task.Run(() => _databaseService.AddLogin(_connectionScope.ServerAddress,
                    _connectionScope.Username, _connectionScope.Password, Username, ConfirmedPassword));

                if (!result.IsSuccessful)
                {
                    Log.Error("Failed creating login, with reason: {resultMessage}", result.Message);
                    _messageBoxHelper.ShowError($"Failed creating login: {result.Message}");
                    return;
                }

                _hasCreatedNewUserInSession = true;

                IsNewUser = false;

                await LoadUserRoles();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed saving login details");

                _messageBoxHelper.ShowError("Something went wrong creating the login, please try again.");
            }
            finally
            {
                _shellManager.SetLoadingData(false);
            }
        }
    }
}
