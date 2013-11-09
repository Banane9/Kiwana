using IRC;
using IRC.Config;
using System;
using System.Threading.Tasks;

namespace Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            IrcServer config = new IrcServer();
            config.Port = 6667;
            config.Url = "irc.esper.net";

            User user = new User();
            user.Password = "iIzBestBot";
            user.Name = "Kiwana";
            user.Nick = "Kiwana";

            config.User = user;

            Client client = new Client(config);

            Task bot = Task.Run(() => { client.Work(); });

            while (!bot.IsCompleted)
            {
                client.ParseLine("a b c :!" + Console.ReadLine());
            }
        }
    }
}
