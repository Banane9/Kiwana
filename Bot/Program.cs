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
                string input = Console.ReadLine();
                if (!bot.IsCompleted)
                {
                    client.ParseLine(input, true);
                }
            }

            Console.WriteLine("Press Enter to close the Command Line");
            Console.ReadLine();
        }
    }
}
