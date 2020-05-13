using Azuser.Services.Model;

namespace Azuser.Client.Views.Explorer
{
    public class DatabaseViewModel
    {
        public DatabaseViewModel(Database database)
        {
            Name = database.Name;
        }

        public string Name { get; set; }
    }
}