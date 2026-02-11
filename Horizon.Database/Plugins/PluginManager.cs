using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Horizon.Database.Plugins
{
    public class PluginManager
    {
        private DirectoryInfo _pluginDir = null;
        private List<IPlugin> _plugins = [];

        public PluginManager(string pluginsDirectory)
        {
            // Ensure valid plugins directory
            if (string.IsNullOrEmpty(pluginsDirectory))
                return;

            this._pluginDir = new DirectoryInfo(pluginsDirectory);
            if (!this._pluginDir.Exists)
                return;

            LoadPlugins();
        }

        public void RegisterPlugins(IConfiguration configuration, IServiceCollection services)
        {
            foreach (var plugin in _plugins)
            {
                plugin.Register(configuration, services);
            }
        }

        private void LoadPlugins()
        {
            // Ensure valid plugins directory
            if (!this._pluginDir.Exists)
                return;

            // Add assemblies
            foreach (var file in this._pluginDir.GetFiles("*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(file.FullName);
                    Type pluginInterface = typeof(IPlugin);
                    var plugins = pluginAssembly.GetTypes()
                        .Where(type => pluginInterface.IsAssignableFrom(type));

                    foreach (var plugin in plugins)
                    {
                        IPlugin instance = (IPlugin)Activator.CreateInstance(plugin);
                        if (instance is null) continue;

                        _plugins.Add(instance);
                        //_ = instance.Start(file.Directory.FullName, this);
                    }
                }
                catch (TypeInitializationException) { }
                catch (BadImageFormatException) { }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
