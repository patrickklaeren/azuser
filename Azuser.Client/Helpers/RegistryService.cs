using System;
using Microsoft.Win32;
using Serilog;

namespace Azuser.Client.Helpers
{
    public interface IRegistryService
    {
        void SetServerAddress(string value);
        string GetServerAddress();
        void SetUsername(string value);
        string GetUsername();
    }

    public class RegistryService : IRegistryService
    {
#if DEBUG
        private const string REGISTRY_DIRECTORY = "SOFTWARE\\AzuserDev";
#else
        private const string REGISTRY_DIRECTORY = "SOFTWARE\\Azuser";
#endif

        private const string SERVER_KEY = "ServerAddress";
        private const string USERNAME_KEY = "Username";

        public void SetServerAddress(string value) => TrySetStringKey(SERVER_KEY, value?.ToLower());

        public string GetServerAddress() => TryGetStringKey(SERVER_KEY);

        public void SetUsername(string value) => TrySetStringKey(USERNAME_KEY, value?.ToLower());

        public string GetUsername() => TryGetStringKey(USERNAME_KEY);

        private static void TrySetStringKey(string key, string value)
        {
            try
            {
                Registry.CurrentUser.CreateSubKey(REGISTRY_DIRECTORY);

                var registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                registryKey?.SetValue(key, value, RegistryValueKind.String);
            }
            catch (Exception e)
            {
                Log.Warning($"Failed setting registry value for key {key}", e);
            }
        }

        private static string TryGetStringKey(string key)
        {
            try
            {
                var registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                if (registryKey == null)
                    return null;

                var value = registryKey.GetValue(key) as string;

                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
            catch (Exception e)
            {
                Log.Warning($"Failed getting registry value for key {key}", e);
            }

            return null;
        }
    }
}
