using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Framework;
using Azuser.Client.Helpers;
using Azuser.Services;
using Serilog;

namespace Azuser.Client.Views.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly IShellManager _shellManager;
        private readonly IDatabaseService _databaseService;
        private readonly IRegistryService _registryService;

        private string _serverAddress;
        private string _username;
        private string _password;
        private bool _rememberServerAndUsername = true;

        private readonly TaskCompletionSource<ConnectionScope> _loginCompletionSource 
            = new TaskCompletionSource<ConnectionScope>();

        public LoginViewModel(IMessageBoxHelper messageBoxHelper, IShellManager shellManager, 
            IDatabaseService databaseService, IRegistryService registryService)
        {
            _messageBoxHelper = messageBoxHelper;
            _shellManager = shellManager;
            _databaseService = databaseService;
            _registryService = registryService;
        }

        public string ServerAddress
        {
            get => _serverAddress;
            set => Set(ref _serverAddress, value);
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

        public bool RememberServerAndUsername
        {
            get => _rememberServerAndUsername;
            set => Set(ref _rememberServerAndUsername, value);
        }

        public Task<ConnectionScope> WaitForLogin()
        {
            ServerAddress = _registryService.GetServerAddress();
            Username = _registryService.GetUsername();

            return _loginCompletionSource.Task;
        }

        public ICommand LoginCommand => new AsyncRelayCommand(Login);

        private string BuildValidation()
        {
            var builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ServerAddress))
            {
                builder.AppendLine("- A valid server address needs to be entered");
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                builder.AppendLine("- A valid username needs to be entered");
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                builder.AppendLine("- A valid password needs to be entered");
            }

            return builder.ToString();
        }

        private async Task Login()
        {
            var validation = BuildValidation();

            if (!string.IsNullOrWhiteSpace(validation))
            {
                _messageBoxHelper.ShowWarning($"Fix the following issues before logging in:\n\n{validation}");
                return;
            }

            try
            {
                _shellManager.SetLoadingData(true);

                // Validate the connection
                var result = await Task.Run(() => _databaseService.ValidateConnection(ServerAddress, Username, Password));

                if (!result.IsSuccessful)
                {
                    if (!string.IsNullOrWhiteSpace(result.Message))
                    {
                        Log.Warning("Failed validating connection to the target server, with message: {message}", result.Message);
                        _messageBoxHelper.ShowError(result.Message, "Failed logging in");
                    }
                    else
                    {
                        Log.Warning("Failed validating connection to the target server for an unknown reason");
                        _messageBoxHelper.ShowError("Something went wrong while attempting to connect to the target server, please try again.", "Failed logging in");
                    }

                    return;
                }

                if (RememberServerAndUsername)
                {
                    _registryService.SetServerAddress(ServerAddress);
                    _registryService.SetUsername(Username);
                }

                // If we've passed validation, add the scope and set the result for the awaiting
                // parent VM
                var scope = new ConnectionScope(ServerAddress, Username, Password);

                _loginCompletionSource.SetResult(scope);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed logging into server");

                _messageBoxHelper.ShowError("Something went wrong, please try again. If the issue persists, check the log.");
            }
            finally
            {
                _shellManager.SetLoadingData(false);
            }
        }
    }
}
