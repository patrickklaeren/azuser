using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Squirrel;

namespace Azuser.Client.Framework
{
    public interface IUpdateService
    {
        Task<bool> TryCheckForUpdate(IProgress<int> progress = default);
        Task<UpdatedVersion> TryUpdate(IProgress<int> progress = default);
        bool TryRestart();
    }

    public class UpdateService : IUpdateService
    {
        private const string UPDATE_URI = "http://inzanit.com/Releases/Azuser/";

        public async Task<bool> TryCheckForUpdate(IProgress<int> progress)
        {
            try
            {
                using (var manager = new UpdateManager(UPDATE_URI))
                {
                    var update = await manager.CheckForUpdate();

                    return update.ReleasesToApply.Any();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed checking for update");
            }

            return false;
        }

        public async Task<UpdatedVersion> TryUpdate(IProgress<int> progress)
        {
            try
            {
                using (var manager = new UpdateManager(UPDATE_URI))
                {
                    var version = await manager.UpdateApp((value) => progress?.Report(value));

                    return new UpdatedVersion(true, version.Filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed checking for update");
            }

            return new UpdatedVersion(false, null);
        }

        public bool TryRestart()
        {
            try
            {
                UpdateManager.RestartApp();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed checking for update");
            }

            return false;
        }
    }

    public class DebugUpdateService : IUpdateService
    {
        public Task<bool> TryCheckForUpdate(IProgress<int> progress = default)
        {
            return Task.FromResult(false);
        }

        public Task<UpdatedVersion> TryUpdate(IProgress<int> progress = default)
        {
            return Task.FromResult(new UpdatedVersion(false, "Debugging"));
        }

        public bool TryRestart()
        {
            return false;
        }
    }

    public class UpdatedVersion
    {
        public UpdatedVersion(bool success, string fileName)
        {
            IsSuccessful = success;
            Path = fileName;
        }

        public bool IsSuccessful { get; set; }
        public string Path { get; set; }
    }
}
