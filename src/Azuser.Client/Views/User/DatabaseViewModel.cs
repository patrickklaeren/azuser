using System.Collections.Generic;
using System.Linq;
using Azuser.Services.Model;

namespace Azuser.Client.Views.User
{
    public class DatabaseViewModel : ViewModelBase
    {
        private bool _isLoadingData;
        private List<RoleViewModel> _roleItemsSource;

        public DatabaseViewModel(Database database)
        {
            Name = database.Name;
        }

        public string Name { get; set; }

        public bool HasLoaded { get; set; }

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set => Set(ref _isLoadingData, value);
        }

        public List<RoleViewModel> RoleItemsSource
        {
            get => _roleItemsSource;
            set => Set(ref _roleItemsSource, value);
        }

        public bool HasBeenModifed => RoleItemsSource?.Any(x => x.HasChanged) == true;
    }
}