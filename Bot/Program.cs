using Kiwana.Core;
using Kiwana.Core.Config;
using Kiwana.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Kiwana
{
    class Program
    {
        static void Main(string[] arg)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BotConfig));
            XmlReader reader = XmlReader.Create("Config/BotConfig.xml");
            BotConfig botConfig = (BotConfig)serializer.Deserialize(reader);

            List<Plugin> plugins = PluginManager.ScanPluginFolder("Plugins");

            Console.WriteLine(plugins[0].Instance.GetRandom(1, 10));

            //Console.WriteLine("Name: " + botConfig.Commands[0].Name);
            //foreach (string alias in botConfig.Commands[0].Alias)
            //{
            //    Console.WriteLine("Alias: " + alias);
            //}

            Client client = new Client(botConfig);

            Task bot = Task.Run(() => { client.Work(); });

            while (!bot.IsCompleted)
            {
                client.ParseLine(Console.ReadLine(), true);
            }
        }
    }
}
