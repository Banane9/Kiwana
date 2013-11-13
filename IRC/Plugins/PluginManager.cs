using Kiwana.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Kiwana.Core.Plugins
{
    public static class PluginManager
    {
        private static XmlSerializer _pluginConfigSerializer = new XmlSerializer(typeof(PluginConfig));

        public static List<Plugin> ScanPluginFolder(string folder)
        {
            List<Plugin> plugins = new List<Plugin>();

            string[] pluginPaths = Directory.GetFiles(folder, "*.dll");

            Console.WriteLine("Loading Plugins from /" + folder + " ...");

            foreach (string pluginPath in pluginPaths)
            {
                Plugin plugin = new Plugin();
                plugin.Name = Regex.Match(pluginPath, @"\w+(?=.dll)", RegexOptions.IgnoreCase).Value;

                Console.WriteLine("  - " + plugin.Name);

                XmlReader reader = XmlReader.Create(folder + "/" + plugin.Name + "Config.xml");
                plugin.Config = (PluginConfig)_pluginConfigSerializer.Deserialize(reader);

                Assembly dll = Assembly.LoadFile(Path.GetFullPath(pluginPath));
                plugin.Instance = (Kiwana.Plugins.Api.Plugin)Activator.CreateInstance(dll.GetType(plugin.Config.ClassName));

                plugins.Add(plugin);
            }

            Console.WriteLine("Done!");

            return plugins;
        }
    }
}
