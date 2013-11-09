using IRC.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IRC
{
    public class Client
    {
        private TcpClient _ircConnection;
        private IrcServer _config;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;


        private Regex _nameRegEx = new Regex(@"(?<=\:)[\w|_\+\^\<\>\[\]]+(?=\!)");
        private Regex _motdRegEx = new Regex(@"(?<=\:)[^\n]+");
        private Regex _messageRegEx = new Regex(@"(?<=\:)[^\n]+");

        private bool _shouldRun = true;

        public Client(IrcServer config)
        {
            _config = config;

            try
            {
                _ircConnection = new TcpClient(_config.Url, _config.Port);
            }
            catch
            {
                Console.WriteLine("Connection Error");
            }

            try
            {
                _networkStream = _ircConnection.GetStream();
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream);

                SendData("PASS", _config.User.Password);
                SendData("NICK", _config.User.Nick);
                SendData("USER", _config.User.Nick + " dafuq.com dafuq.com :" + _config.User.Name);
            }
            catch
            {
                Console.WriteLine("Communication Error");
            }
        }

        public void SendData(string command, string argument = "")
        {
            if (argument == "")
            {
                _streamWriter.WriteLine(command);
                _streamWriter.Flush();
            }
            else
            {
                _streamWriter.WriteLine(command + " " + argument);
                _streamWriter.Flush();
            }

            Console.WriteLine(command + " " + argument);
        }

        public void Work()
        {
            _shouldRun = true;
            
            while(_shouldRun)
            {
                ParseLine(_streamReader.ReadLine());
            }
        }

        public void ParseLine(string line)
        {
            List<string> ex = line.Split(' ').ToList();

            if (ex[0] == "PING")
            {
                Console.WriteLine("PING " + ex[1]);
                SendData("PONG", ex[1]);
            }

            if (ex.Count > 3)
            {
                if (_nameRegEx.IsMatch(ex[0]))
                {
                    Console.WriteLine(ex[2] + " " + ex[1] + " <" + _nameRegEx.Match(ex[0]) + "> " + _messageRegEx.Match(ex[3]) + " " + _joinStringArray(ex, " "));
                }
                else if (_motdRegEx.IsMatch(ex[3]))
                {
                    Console.WriteLine(_motdRegEx.Match(ex[3]) + " " + _joinStringArray(ex, " "));
                }
                else if (_messageRegEx.IsMatch(_joinStringArray(ex, " ")))
                {
                    Console.WriteLine(_messageRegEx.Match(_joinStringArray(ex, " ")));
                }
                else
                {
                    Console.WriteLine(ex[0] + " " + ex[1] + " " + ex[2] + " " + ex[3] + " " + _joinStringArray(ex, " "));
                }

                //Commands with arguments
                if (ex.Count > 4)
                {
                    switch (ex[3])
                    {
                        case ":!join":
                            SendData("JOIN", ex[4]);
                            break;
                        case ":!say":
                            if (ex[4].First() == '#')
                            {
                                string chan = ex[4];
                                ex.Remove(ex[4]);
                                ex[2] = chan;
                            }
                            SendData("PRIVMSG", ex[2] + " :" + _joinStringArray(ex, " ")); //channel + *space*: + message
                            break;
                        case ":!quit":
                            SendData("QUIT", ":" + _joinStringArray(ex, " "));
                            _shouldRun = false;
                            break;
                    }
                }
                else //Commands without arguments
                {
                    switch (ex[3])
                    {
                        case ":!part":
                            SendData("PART", ex[2]);
                            break;
                        case ":!quit":
                            SendData("QUIT");
                            _shouldRun = false;
                            break;
                    }
                }
            }
        }

        private string _joinStringArray(List<string> strings, string glue = "")
        {
            string str = "";

            for (int i = 4; i < strings.Count; i++)
            {
                if (i < strings.Count && i > 4)
                { str += glue; }

                str += strings[i];
            }
            return str;
        }
    }
}
