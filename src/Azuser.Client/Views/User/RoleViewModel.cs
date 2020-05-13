using Azuser.Services.Model;

namespace Azuser.Client.Views.User
{
    public class RoleViewModel : ViewModelBase
    {
        private bool _value;
        private readonly bool _originalValue;

        public RoleViewModel(DatabaseRole databaseRole)
        {
            Name = databaseRole.Name;
            Value = databaseRole.Value;
            RawSqlRoleName = databaseRole.RawSqlRoleName;

            _originalValue = databaseRole.Value;
        }

        public string RawSqlRoleName { get; }

        public string Name { get; set; }

        public bool Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        public bool HasChanged => _originalValue != Value;

        public bool IsRemoved => _originalValue && Value == false;
    }
}