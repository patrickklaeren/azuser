using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using MahApps.Metro.Controls;

namespace Azuser.Client.Framework.Resolver
{
    public class AssemblyScanningModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ScanAssemblies(builder);

            base.Load(builder);
        }

        private static void ScanAssemblies(ContainerBuilder builder)
        {
            var assemblies = new List<Assembly>();

            // Add the calling assembly
            assemblies.Add(Assembly.GetEntryAssembly());

            // Now add all the referenced assemblies
            assemblies.AddRange(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "Azuser.*.dll", SearchOption.AllDirectories)
                .Select(Assembly.LoadFrom));

            foreach (var assembly in assemblies)
            {
                // Register all ViewModels
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.IsAbstract == false)
                    .Where(t => t.IsSubclassOf(typeof(Views.ViewModelBase)))
                    .Where(t => t.Name.EndsWith("ViewModel"))
                    .AsSelf();

                // Register all Views
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.IsAbstract == false)
                    .Where(t => t.IsSubclassOf(typeof(MetroWindow)))
                    .AsSelf();

                // Bind all classes to their defacto interface 
                // (if one is available with the same name)
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.IsClass && t.GetInterfaces().Any(d => d.Name.EndsWith(t.Name)))
                    .As(t => t.GetInterfaces().First(i => i.Name.EndsWith(t.Name)));
            }
        }
    }
}
