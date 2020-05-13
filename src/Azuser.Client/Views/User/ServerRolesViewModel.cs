using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Helpers;
using Azuser.Services;
using Azuser.Services.Model;
using Serilog;

namespace Azuser.Client.Views.User
{
    public class ServerRolesViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IShellManager _shellManager;
        private readonly IMessageBoxHelper _messageBoxHelper;

        private ConnectionScope _connectionScope;
        private DatabaseViewModel _selectedDatabase;
        private List<DatabaseViewModel> _databaseItemsSource;
        private string _username;

        public ServerRolesViewModel(IDatabaseService databaseService,
            IShellManager shellManager, IMessageBoxHelper messageBoxHelper)
        {
            _databaseService = databaseService;
            _shellManager = shellManager;
            _messageBoxHelper = messageBoxHelper;
        }

        public List<DatabaseViewModel> DatabaseItemsSource
        {
            get => _databaseItemsSource;
            set => Set(ref _databaseItemsSource, value);
        }

        public DatabaseViewModel SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                if (Set(ref _selectedDatabase, value))
                {
                    // Fire and forget
                    _ = LoadDatabase(value);
                }
            }
        }

        public async Task InitializeAsync(string username, ConnectionScope connectionScope)
        {
            _connectionScope = connectionScope;

            _username = username;

            _shellManager.SetLoadingData(true);

            var databases = await Task.Run(() => _databaseService.GetDatabasesForServer(connectionScope.ServerAddress,
                connectionScope.Username, connectionScope.Password));

            DatabaseItemsSource = databases.Select(x => new DatabaseViewModel(x)).ToList();

            _shellManager.SetLoadingData(false);

            SelectedDatabase = DatabaseItemsSource.FirstOrDefault();
        }

        private async Task LoadDatabase(DatabaseViewModel database)
        {
            if (database.HasLoaded)
                return;

            try
            {
                database.IsLoadingData = true;

                var roles = await Task.Run(() => _databaseService.GetRolesForUserInDatabase(_connectionScope.ServerAddress, _connectionScope.Username,
                    _connectionScope.Password, database.Name, _username));

                database.RoleItemsSource = roles.Select(x => new RoleViewModel(x)).ToList();

                database.HasLoaded = true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed loading roles for user in database");

                _messageBoxHelper.ShowError("Something went wrong while loading the roles for the selected user, please try again. If the issue persists, check the log.");
            }
            finally
            {
                database.IsLoadingData = false;
            }
        }

        public async Task Save()
        {
            if (!DatabaseItemsSource.Any(x => x.HasBeenModifed))
                return;

            foreach (var database in DatabaseItemsSource.Where(x => x.HasBeenModifed))
            {
                try
                {
                    foreach (var role in database.RoleItemsSource.Where(x => x.HasChanged))
                    {
                        OperationResult result;

                        if (role.IsRemoved)
                        {
                            result = await Task.Run(() => _databaseService.DeleteRoleForUser(_connectionScope.ServerAddress,
                                _connectionScope.Username, _connectionScope.Password, database.Name, _username,
                                role.RawSqlRoleName));
                        }
                        else
                        {
                            result = await Task.Run(() => _databaseService.AddRoleForUser(_connectionScope.ServerAddress,
                                _connectionScope.Username, _connectionScope.Password, database.Name, _username,
                                role.RawSqlRoleName));
                        }

                        if (!result.IsSuccessful)
                        {
                            Log.Warning("Failed modifying role {role} for user in database with message: {resultMessage}", role.RawSqlRoleName, result.Message);

                            var confirmation = _messageBoxHelper.ShowDialog(
                                $"Something went wrong while modifying the role for this user:\n\n{result.Message}. Would you like to skip this error? Press cancel to stop processing all roles.",
                                "An error has occurred", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);

                            if (confirmation != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed modifying role in database {name}", database.Name);

                    var confirmation = _messageBoxHelper.ShowDialog(
                        $"Something went wrong while modifying the target database. Would you like to skip this error? By pressing No, the current database ({database.Name}) will be skipped. Press cancel to stop processing all databases.",
                        "An error has occurred", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);

                    if (confirmation == MessageBoxResult.No)
                    {
                        continue;
                    }

                    if (confirmation != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }
        }
    }
}
