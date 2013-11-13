using Kiwana.Core;
using Kiwana.Core.Config;
using System;
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
            XmlReader reader = XmlReader.Create("BotConfig.xml");
            BotConfig botConfig = (BotConfig)serializer.Deserialize(reader);

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
