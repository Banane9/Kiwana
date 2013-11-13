using IRC.Commands;
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
        private BotConfig _config;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private Regex _prefixRegex;
        private Regex _serverCommandRegex;
        private Regex _consoleCommandRegex;
        private Regex _nickRegex = new Regex(@"(?<=\:).+(?=\!)");
        private Regex _nameRegex = new Regex(@"(?<=!|!~)[^~].+(?=@)");
        private Regex _motdRegex = new Regex(@"(?<=\:).+");
        private Regex _messageRegex = new Regex(@"(?<=\:).+");

        private Random random = new Random();

        private bool _shouldRun = true;

        public Client(BotConfig config)
        {
            _config = config;

            _prefixRegex = new Regex(@"(?<=" + _joinStringArray(_config.Prefixes, "|") + ").+");

            string serverCommandRegex = @"";
            string consoleCommandRegex = @"";
            for (int i = 0; i < _config.Commands.Count; i++)
            {
                Command command = _config.Commands[i];

                if (command.ConsoleServer == ConsoleServer.Server || command.ConsoleServer == ConsoleServer.Both)
                {
                    serverCommandRegex += command.Name + "|";
                    string alias = _joinStringArray(command.Alias, "|");
                    if (!String.IsNullOrEmpty(alias))
                    {
                        serverCommandRegex += alias;
                        if (i < _config.Commands.Count - 1)
                        {
                            serverCommandRegex += "|";
                        }
                    }
                }

                if (command.ConsoleServer == ConsoleServer.Console || command.ConsoleServer == ConsoleServer.Both)
                {
                    consoleCommandRegex += command.Name + "|";
                    string alias = _joinStringArray(command.Alias, "|");
                    if (!String.IsNullOrEmpty(alias))
                    {
                        consoleCommandRegex += alias;
                        if (i < _config.Commands.Count - 1)
                        {
                            consoleCommandRegex += "|";
                        }
                    }
                }
            }
            _serverCommandRegex = new Regex(serverCommandRegex);
            _consoleCommandRegex = new Regex(consoleCommandRegex);

            try
            {
                _ircConnection = new TcpClient(_config.Server.Url, _config.Server.Port);
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

                SendData("PASS", _config.Server.User.Password);
                SendData("NICK", _config.Server.User.Name);
                SendData("USER", _config.Server.User.Nick + " Banane 9 :" + _config.Server.User.Name);
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

        public void ParseLine(string line, bool console = false)
        {
            List<string> ex = line.Split(' ').ToList();

            if (ex[0] == "PING")
            {
                Console.WriteLine("PING " + ex[1]);
                SendData("PONG", ex[1]);
            }

            if (ex.Count > 3 || console)
            {
                string command = "";

                if (console)
                {
                    command = ex[0].ToLower();
                    ex.InsertRange(0, new string[] { "", "", "" });
                }
                else
                {
                    command = _prefixRegex.Match(ex[3]).Value.ToLower();
                }

                //Print input from server
                if (!console)
                {
                    //Console.WriteLine(_joinStringArray(ex, " "));
                    if (_nickRegex.IsMatch(ex[0]))
                    {
                        Console.WriteLine(ex[2] /*+ " " + ex[1]*/ + " <" + _nickRegex.Match(ex[0]) + "!" + _nameRegex.Match(ex[0]) + "> " + _messageRegex.Match(ex[3]) + " " + _joinStringArray(ex, " ", 4));
                    }
                    else if (_motdRegex.IsMatch(ex[3]))
                    {
                        Console.WriteLine(_motdRegex.Match(ex[3]) + " " + _joinStringArray(ex, " ", 4));
                    }
                    else if (_messageRegex.IsMatch(_joinStringArray(ex, " ", 4)))
                    {
                        Console.WriteLine(_messageRegex.Match(_joinStringArray(ex, " ", 4)));
                    }
                    else
                    {
                        Console.WriteLine(_joinStringArray(ex, " "));
                    }
                }

                //Is it a valid command from the console or from the server
                if ((_consoleCommandRegex.IsMatch(command) && console) || (_serverCommandRegex.IsMatch(command) && !console))
                {
                    string normalizedCommand = "";
                    foreach (Command commandToCheck in _config.Commands)
                    {
                        if (command == commandToCheck.Name)
                        {
                            normalizedCommand = command;
                            break;
                        }

                        string regexString = "^(" + _joinStringArray(commandToCheck.Alias, "|") + ")$";

                        if (!String.IsNullOrEmpty(regexString))
                        {
                            Regex regex = new Regex(regexString);
                            if (regex.IsMatch(command))
                            {
                                normalizedCommand = regex.Replace(command, commandToCheck.Name);
                            }

                            if (!String.IsNullOrEmpty(normalizedCommand)) break;
                        }
                    }

                    if (ex[2] == _config.Server.User.Nick)
                    {
                        ex[2] = _nickRegex.Match(ex[0]).Value;
                    }

                    //Commands with arguments
                    if (ex.Count > 4)
                    {
                        switch (normalizedCommand)
                        {
                            case "join":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("JOIN", _joinStringArray(ex, ",", 4));
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "about":
                                SendData("PRIVMSG", ex[2] + " :" + _config.About);
                                break;
                            case "help":
                                string help = "Available commands are: ";
                                help += _joinStringArray(_config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ");
                                help += " . With prefixes: " + _joinStringArray(_config.Prefixes, ", ") + " .";
                                SendData("PRIVMSG", ex[2] + " :" + help.Replace("\\", ""));
                                break;
                            case "say":
                                SendData("PRIVMSG", ex[2] + " :" + _joinStringArray(ex, " ", 4)); //channel + *space*: + message
                                break;
                            case "letmegooglethatforyou":
                                SendData("PRIVMSG", ex[2] + " :http://lmgtfy.com/?q=" + _joinStringArray(ex, "+", 4));
                                break;
                            case "random":
                                try
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": " + random.Next(int.Parse(Regex.Match(ex[4], @"\d+").Value), int.Parse(Regex.Match(ex[5], @"\d+").Value)));
                                }
                                catch
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": Couldn't parse numbers.");
                                }
                                break;
                            case "dice":
                                try
                                {
                                    int value = 0;
                                    string nextMode = "+";

                                    if (Regex.IsMatch(_joinStringArray(ex, start: 4), @"\d+((-|\+)\d+)?d\d+(-|\+)?"))
                                    {
                                        MatchCollection diceFormulas = Regex.Matches(_joinStringArray(ex, start: 4), @"\d+((-|\+)\d+)?d\d+(-|\+)?");
                                        foreach (Match diceFormula in diceFormulas)
                                        {
                                            Console.WriteLine(diceFormula.Value);

                                            int diceValue = 0;

                                            int dice = int.Parse(Regex.Match(diceFormula.Value, @"^\d+").Value);

                                            int add = 0;
                                            if (Regex.IsMatch(ex[4], @"(-|\+)\d+(?=d)"))
                                            {
                                                add = int.Parse(Regex.Match(diceFormula.Value, @"(-|\+)\d+(?=d)").Value);
                                            }

                                            int max = int.Parse(Regex.Match(diceFormula.Value, @"(?<=d)\d+").Value);

                                            for (int i = 0; i < dice; i++)
                                            {
                                                diceValue += random.Next(1, max);
                                            }

                                            diceValue += add;

                                            switch (nextMode)
                                            {
                                                case "+":
                                                    value += diceValue;
                                                    break;
                                                case "-":
                                                    value -= diceValue;
                                                    break;
                                            }

                                            nextMode = Regex.Match(diceFormula.Value, @"(-|\+)$").Value;
                                        }

                                        SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": " + value);
                                    }
                                    else
                                    {
                                        SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": No valid dice formula found.");
                                    }
                                }
                                catch
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": Couldn't parse dice formula.");
                                }
                                break;
                            case "tell":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("PRIVMSG", ex[4] + " :" + _joinStringArray(ex, " ", 5));
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "nick":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("NICK", _joinStringArray(ex, "_", 4));
                                    _config.Server.User.Nick = _joinStringArray(ex, "_", 4);
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "raw":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    _shouldRun = ex[4].ToUpper() != "QUIT";
                                    SendData(_joinStringArray(ex, " ", 4));
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "part":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("PART", _joinStringArray(ex, ",", 4));
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "quit":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("QUIT", ":" + _joinStringArray(ex, " ", 4));
                                    _shouldRun = false;
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            default:
                                SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": Command not recognized.");
                                break;
                        }
                    }
                    else //Commands without arguments
                    {
                        switch (normalizedCommand)
                        {
                            case "about":
                                SendData("PRIVMSG", ex[2] + " :" + _config.About);
                                break;
                            case "help":
                                string help = "Available commands are: ";
                                help += _joinStringArray(_config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ");
                                help += " . With prefixes: " + _joinStringArray(_config.Prefixes, ", ") + " .";
                                SendData("PRIVMSG", ex[2] + " :" + help.Replace("\\", ""));
                                break;
                            case "tosscoin":
                                SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": " + (random.Next(1, 3) == 1 ? "Tails" : "Heads"));
                                break;
                            case "mcstatus":
                                SendData("PRIVMSG", ex[2] + " :Retrieving status of Minecraft services...");
                                _writeMcStatus(ex[2]);
                                break;
                            case "part":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("PART", ex[2]);
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                            case "quit":
                                if (_canDoCommand(_nickRegex.Match(ex[0]).Value) || console)
                                {
                                    SendData("QUIT");
                                    _shouldRun = false;
                                }
                                else
                                {
                                    SendData("PRIVMSG", ex[2] + " :" + _nickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                                }
                                break;
                        }
                    }
                }
            }
        }

        private async void _writeMcStatus(string who)
        {
            Dictionary<string, string> mcStatus = await McStatus.GetMcStatus();

            foreach (string key in mcStatus.Keys.ToList())
            {
                SendData("PRIVMSG", who + " :" + McStatus.SericeNames[key] + ": " + McStatus.StatusMessages[mcStatus[key]]);
            }
        }

        private bool _canDoCommand(string name)
        {
            bool inList = false;
            foreach (string user in _config.Users)
            {
                if (user == name)
                {
                    inList = true;
                    break;
                }
            }

            if (!inList) return false;

            return GetAuthenticationStatus(name) == 3;
        }

        public int GetAuthenticationStatus(string name)
        {
            SendData("PRIVMSG", "NickServ :ACC " + name);
            List<string> ex = _streamReader.ReadLine().Split(' ').ToList();
            if (ex.Count == 6 && _nickRegex.Match(ex[0]).Value == "NickServ")
            {
                try
                {
                    return int.Parse(ex[5]);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private string _joinStringArray(List<string> strings, string glue = "", int start = 0)
        {
            string str = "";

            for (int i = start; i < strings.Count; i++)
            {
                if (i < strings.Count && i > start)
                { str += glue; }

                str += strings[i];
            }
            return str;
        }
    }
}
