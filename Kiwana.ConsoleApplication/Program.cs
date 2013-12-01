using Kiwana;
using Kiwana.Config;
using Kiwana.Plugins;
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
            Client client = new Client("Config/BotConfig.xml");

            Task bot = Task.Run(() => client.Work());

            while (client.Running)
            {
                string input = Console.ReadLine();
                if (client.Running)
                {
                    client.ParseLine(input, true);
                }
            }
        }
    }
}
