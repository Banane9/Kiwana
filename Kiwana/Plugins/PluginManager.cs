using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kiwana.Plugins
{
    /// <summary>
    /// The class used to load the plugin DLLs.
    /// </summary>
    public static class PluginManager
    {
        /// <summary>
        /// Scans a plugin folder and returns the Plugins found.
        /// </summary>
        /// <param name="folder">The path to the folder to scan.</param>
        /// <returns>Any plugins found as a <see cref="Dictionary"/> that matches the Plugin's instance to its name.</returns>
        public static Dictionary<string, Plugin> ScanPluginFolder(string folder)
        {
            Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

            if (Directory.Exists(folder))
            {
                string[] pluginPaths = Directory.GetFiles(folder, "*.dll");

                foreach (string pluginPath in pluginPaths)
                {
                    Assembly dll = Assembly.LoadFile(Path.GetFullPath(pluginPath));
                    foreach (Type type in dll.ExportedTypes)
                    {
                        if (type.IsSubclassOf(typeof(Plugin)))
                        {
                            plugins.Add(type.Name, (Plugin)Activator.CreateInstance(type));
                        }
                    }
                }
            }

            return plugins;
        }
    }
}
