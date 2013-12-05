using Kiwana;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Kiwana.ConsoleApplication
{
    /// <summary>
    /// The Class that runs the bot inside a Console Application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The method that is executed when the program starts.
        /// </summary>
        /// <param name="arg">Arguments passed to the method.</param>
        static void Main(string[] arg)
        {
            Console.Title = "Kiwana";

            Kiwana client = new Kiwana("Config/BotConfig.xml");

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
