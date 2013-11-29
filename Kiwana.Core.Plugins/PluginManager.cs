using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Kiwana.Core
{
    public static class PluginManager
    {
        public static List<Type> ScanPluginFolder(string folder)
        {
            List<Type> types = new List<Type>();

            string[] plugins = Directory.GetFiles(folder, "*.dll");

            Console.WriteLine("Loading Plugins...");

            foreach (string plugin in plugins)
            {
                string pluginName = Regex.Match(plugin, @"\w+(?=.dll)", RegexOptions.IgnoreCase).Value;
                Console.WriteLine("  - " + pluginName);
                Assembly dll = Assembly.LoadFile(Path.GetFullPath(plugin));
            }

            return types;
        }
    }
}
