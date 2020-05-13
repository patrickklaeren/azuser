namespace Azuser.Services.Model
{
    public sealed class Database
    {
        internal Database(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}