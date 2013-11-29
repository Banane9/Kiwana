using Kiwana.Core.Api;
using Kiwana.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Kiwana.Core.Plugins
{
    public static class PluginManager
    {
        private static XmlSerializer _pluginConfigSerializer = new XmlSerializer(typeof(PluginConfig));

        public static List<PluginInformation> ScanPluginFolder(string folder)
        {
            List<PluginInformation> plugins = new List<PluginInformation>();

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
                            PluginInformation plugin = new PluginInformation();
                            plugin.Name = type.Name;

                            XmlReader reader = XmlReader.Create(folder + "/" + plugin.Name + "Config.xml");
                            plugin.Config = (PluginConfig)_pluginConfigSerializer.Deserialize(reader);

                            plugin.Instance = (Plugin)Activator.CreateInstance(type);

                            plugins.Add(plugin);
                        }
                    }
                }
            }

            return plugins;
        }
    }
}
