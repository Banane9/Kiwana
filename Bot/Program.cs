using Kiwana.Core;
using Kiwana.Core.Config;
using Kiwana.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Kiwana.ConsoleApplication
{
    class Program
    {
        static void Main(string[] arg)
        {
            Client client = new Client("BotConfig.xml");

            Task bot = Task.Run(() => client.Work());

            while (!bot.IsCompleted)
            {
                client.ParseLine(Console.ReadLine(), true);
            }
            Console.ReadLine();
        }
    }
}
