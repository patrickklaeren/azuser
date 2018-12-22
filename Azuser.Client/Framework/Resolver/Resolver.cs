using System;
using Autofac;
using Azuser.Client.DatabaseScopes;
using Azuser.Client.Helpers;

namespace Azuser.Client.Framework.Resolver
{
    public interface IResolver
    {
        T Get<T>();
    }

    public class Resolver : IResolver
    {
        private static IContainer _container;

        public static void Initialise()
        {
            if (_container != null)
                throw new InvalidOperationException($"Cannot initialize {nameof(Resolver)} multiple times");

            var builder = new ContainerBuilder();

            builder.RegisterModule(new AssemblyScanningModule());

            builder.RegisterType<MessageBoxHelper>().As<IMessageBoxHelper>();
            builder.RegisterType<ShellManager>().As<IShellManager>();

#if DEBUG
            builder.RegisterType<DebugUpdateService>().As<IUpdateService>();
#else
            builder.RegisterType<UpdateService>().As<IUpdateService>();
#endif

            builder.RegisterType<Resolver>().As<IResolver>();

            _container = builder.Build();
        }

        /// <summary>
        /// Static member to resolve registered dependencies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Resolve<T>();
        }

        T IResolver.Get<T>()
        {
            return Resolve<T>();
        }

        private static T Resolve<T>()
        {
            if (_container == null)
                throw new NullReferenceException($"{nameof(Resolver)} not initialized");

            return _container.Resolve<T>();
        }
    }
}
