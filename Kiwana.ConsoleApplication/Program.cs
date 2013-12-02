using Kiwana;
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
            Console.Title = "Kiwana";

            Client client = new Client("Config/BotConfig.xml");

            Task bot = Task.Run(() => client.Work());

            bool commandSendBeforeTermination = false;

            while (client.Running)
            {
                commandSendBeforeTermination = false;

                string input = Console.ReadLine();
                if (client.Running)
                {
                    client.ParseLine(input, true);
                    commandSendBeforeTermination = true;
                }
            }

            if (commandSendBeforeTermination)
            {
                Console.ReadLine();
            }
        }
    }
}
