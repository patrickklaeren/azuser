using System;
using System.Threading.Tasks;
using System.Windows;
using Azuser.Client.Framework;
using Azuser.Client.Framework.Resolver;
using Azuser.Client.Views.Shell;
using Azuser.Client.Views.Updater;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Json;

namespace Azuser.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Logger = CreateLogger();

            Log.Information($"Initializing Azuser with version {Version}");

            Log.Debug("Created logger");

            SubscribeForUnhandledExceptions();

            Log.Debug("Subscribed to unhandled exceptions");

            Resolver.Initialise();

            Log.Debug("Resolver initialized");

            Log.Information("Checking for update");
            var window = Resolver.Get<Updater>();

            var dataContext = (UpdaterViewModel)window.DataContext;

            var progress = new Progress<int>(value => dataContext.CurrentProgress = value);

            window.Show();
            return;

            var hasUpdated = await TryUpdate();

            if (hasUpdated)
            {
                return;
            }

            var shell = Resolver.Get<Shell>();

            shell.Show();

            Log.Debug("Shell initialized");
        }

        private static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(new JsonFormatter(), "logs\\log-{Date}.txt")
                .CreateLogger();
        }

        private static void SubscribeForUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledDomainException;
            TaskScheduler.UnobservedTaskException += OnUnhandledTaskSchedulerException;
        }

        private static void OnUnhandledDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Fatal(exception, "Client experienced unhandled exception");
        }

        private static void OnUnhandledTaskSchedulerException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Client experienced unhandled exception in task scheduler");
        }

        private static async Task<bool> TryUpdate()
        {
            var updateService = Resolver.Get<IUpdateService>();

            var updateAvailable = await updateService.TryCheckForUpdate();

            if (!updateAvailable)
                return false;

            var window = Resolver.Get<Updater>();

            var dataContext = (UpdaterViewModel) window.DataContext;

            var progress = new Progress<int>(value => dataContext.CurrentProgress = value);

            window.Show();

            var updated = await updateService.TryUpdate(progress);

            if (updated.IsSuccessful)
            {
                updateService.TryRestart();
            }

            return updated.IsSuccessful;
        }
    }
}
