using Kiwana.Core.Config;
using Kiwana.Core.Objects;
using Kiwana.Core.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kiwana.Core.Plugins;
using System.Xml.Serialization;
using System.Xml;

namespace Kiwana.Core
{
    public class Client
    {
        public bool Initialized = false;
        public bool Running = true;

        private TcpClient _ircConnection;
        private BotConfig _config;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private List<PluginInformation> _plugins = new List<PluginInformation>();

        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();

        private Regex _prefixRegex;
        private Regex _serverCommandRegex;
        private Regex _consoleCommandRegex;

        private Random random = new Random();

        public Client(string config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BotConfig));
            XmlReader reader = XmlReader.Create(config);
            _config = (BotConfig)serializer.Deserialize(reader);
        }

        public void Init()
        {
            foreach (string pluginFolder in _config.PluginFolders)
            {
                _plugins.AddRange(PluginManager.ScanPluginFolder(pluginFolder));
            }

            Console.WriteLine("Initializing Plugins ...");
            foreach (PluginInformation plugin in _plugins)
            {
                Console.Write("  - " + plugin.Name + " ... ");
                try
                {
                    plugin.Instance.Init();
                    Console.WriteLine("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failure");
                    throw new PluginInitializationFailedException("Plugin " + plugin.Name + " failed initalization.", e);
                }
            }

            _prefixRegex = new Regex(@"(?<=" + Util.JoinStringList(_config.Prefixes, "|") + ").+");

            //Add all the listeners to the NewLine event
            NewLine += _handleLine;
            foreach (PluginInformation plugin in _plugins)
            {
                _config.Commands.AddRange(plugin.Config.Commands);
                plugin.Instance.SendDataEvent += SendData;
                NewLine += plugin.Instance.HandleLine;
            }

            string serverCommandRegex = @"";
            string consoleCommandRegex = @"";
            for (int i = 0; i < _config.Commands.Count; i++)
            {
                Command command = _config.Commands[i];

                if (command.ConsoleServer == ConsoleServer.Server || command.ConsoleServer == ConsoleServer.Both)
                {
                    serverCommandRegex += command.Name + "|";
                    string alias = Util.JoinStringList(command.Alias, "|");
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
                    string alias = Util.JoinStringList(command.Alias, "|");
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

            Initialized = true;
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

        public async Task Work()
        {
            if (!Initialized)
            {
                Init();
            }

            Console.WriteLine("Trying to establish Connection to " + _config.Server.Url + ":" + _config.Server.Port + " ... ");
            try
            {
                _ircConnection = new TcpClient(_config.Server.Url, _config.Server.Port);
                _networkStream = _ircConnection.GetStream();
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream);

                SendData("PASS", _config.Server.User.Password);
                SendData("NICK", _config.Server.User.Name);
                SendData("USER", _config.Server.User.Nick + " Owner Banane9 :" + _config.Server.User.Name);

                Console.WriteLine("Success");
                Running = true;
            }
            catch
            {
                Console.WriteLine("Failure");
                Running = false;
            }
            
            while(Running)
            {
                try
                {
                    ParseLine(_streamReader.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public string GetNormalizedCommand(string cmd, bool console)
        {
            string command = "";

            //Is it a valid command from the console or from the server
            if ((_consoleCommandRegex.IsMatch(cmd) && console) || (_serverCommandRegex.IsMatch(cmd) && !console))
            {
                foreach (Command commandToCheck in _config.Commands)
                {
                    if (cmd == commandToCheck.Name)
                    {
                        command = cmd;
                        break;
                    }

                    string regexString = "^(" + Util.JoinStringList(commandToCheck.Alias, "|") + ")$";

                    if (!String.IsNullOrEmpty(regexString))
                    {
                        Regex regex = new Regex(regexString);
                        if (regex.IsMatch(cmd))
                        {
                            command = regex.Replace(cmd, commandToCheck.Name);
                        }

                        if (!String.IsNullOrEmpty(command)) break;
                    }
                }
            }

            return command;
        }

        public void ParseLine(string line, bool console = false)
        {
            List<string> ex = line.Split(' ').ToList();
            //Console.WriteLine(Util.JoinStringList(ex, " "));

            if (ex.Count < 2) return;

            if (!console)
            {
                if (ex.Count == 2)
                {
                    if (ex[0] == "PING")
                    {
                        Console.WriteLine("PING " + ex[1]);
                        SendData("PONG", ex[1]);
                    }
                }

                if (ex.Count > 6)
                {
                    if (Util.ServerRegex.IsMatch(ex[0]))
                    {
                        if (ex[3] == "=")
                        {
                            //If the bot is in the channel
                            if (Channels.ContainsKey(ex[4].ToLower()))
                            {
                                Console.WriteLine("Users in " + ex[4] + ": " + Util.MessageRegex.Match(ex[5]).Value + ", " + Util.JoinStringList(ex, ", ", 6));

                                //Dictionary for the Users in the Channel
                                Dictionary<string, ChannelUser> userList = new Dictionary<string, ChannelUser>(ex.Count - 6);

                                //Add the usernames from the list; first name needs to get the colon at the start stripped.
                                userList.Add(Util.MessageRegex.Match(ex[5]).Value, new ChannelUser());
                                foreach (string userName in ex.GetRange(6, ex.Count - 6))
                                {
                                    userList.Add(userName, new ChannelUser());
                                }

                                //Set it in the dictionary
                                Channels[ex[4].ToLower()].Users = userList;
                            }
                        }
                    }
                }

                if (ex.Count > 5)
                {
                    if (Util.HostMaskRegex.IsMatch(ex[4]))
                    {
                        if (Channels.ContainsKey(ex[3].ToLower()))
                        {
                            Channels[ex[3].ToLower()].MotdSetter = new ChannelUser(hostMask: ex[4]);
                            Channels[ex[3].ToLower()].MotdSetDate = new DateTime(long.Parse(ex[5]));

                            Console.WriteLine("Motd of " + ex[3] + " was set by " + Util.NickRegex.Match(ex[4]) + " at " + Channels[ex[3].ToLower()].MotdSetDate.Hour + ":" + Channels[ex[3].ToLower()].MotdSetDate.Minute + " on " + Channels[ex[3].ToLower()].MotdSetDate.Day + "." + Channels[ex[3].ToLower()].MotdSetDate.Month + "." + Channels[ex[3].ToLower()].MotdSetDate.Year);
                        }
                    }
                }

                if (ex.Count > 4)
                {
                    if (Util.MessageRegex.IsMatch(ex[4]) && ex[4] != ":End")
                    {
                        if (Channels.ContainsKey(ex[3].ToLower()))
                        {
                            Channels[ex[3].ToLower()].Motd = Util.MessageRegex.Match(ex[4] + " " + Util.JoinStringList(ex, " ", 5)).Value;

                            Console.WriteLine("Motd of " + ex[3] + " is: " + Channels[ex[3].ToLower()].Motd);
                        }
                    }
                }

                if (ex.Count == 3)
                {
                    if (ex[1] == "JOIN")
                    {
                        if (Channels.ContainsKey(ex[2].ToLower()))
                        {
                            Channels[ex[2].ToLower()].Users.Add(Util.NickRegex.Match(ex[0]).Value, new ChannelUser(hostMask: Util.HostMaskRegex.Match(ex[0]).Value));

                            Console.WriteLine(ex[2] + " " + Util.NickRegex.Match(ex[0]).Value + " joined the channel.");
                        }
                    }
                }

                if (ex.Count > 2)
                {
                    if (ex[1] == "PART")
                    {
                        Channels[ex[2].ToLower()].Users.Remove(Util.NickRegex.Match(ex[0]).Value);

                        Console.WriteLine(ex[2] + " " + Util.NickRegex.Match(ex[0]).Value + " left the channel.");
                    }
                    else if (ex[1] == "QUIT")
                    {
                        foreach (string channel in Channels.Keys)
                        {
                            Channels[channel].Users.Remove(Util.NickRegex.Match(ex[0]).Value);
                        }

                        Console.WriteLine(Util.NickRegex.Match(ex[0]).Value + " left the server.");
                    }
                }

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
                    if (Util.NickRegex.IsMatch(ex[0]))
                    {
                        Console.WriteLine(ex[2] + " <" + Util.NickRegex.Match(ex[0]) + "!" + Util.NameRegex.Match(ex[0]) + "> " + Util.MessageRegex.Match(ex[3]) + " " + Util.JoinStringList(ex, " ", 4));
                    }
                    else if (Util.MessageRegex.IsMatch(ex[3]))
                    {
                        Console.WriteLine(Util.MessageRegex.Match(ex[3]) + " " + Util.JoinStringList(ex, " ", 4));
                    }
                    else if (Util.MessageRegex.IsMatch(Util.JoinStringList(ex, " ", 4)))
                    {
                        Console.WriteLine(Util.MessageRegex.Match(Util.JoinStringList(ex, " ", 4)));
                    }
                }

                string normalizedCommand = GetNormalizedCommand(command, console);

                if (ex[2] == _config.Server.User.Nick)
                {
                    ex[2] = Util.NickRegex.Match(ex[0]).Value;
                }

                NewLine(ex, normalizedCommand, false, console);
            }
        }

        private void _handleLine(List<string> ex, string command, bool userAuthenticated, bool console)
        {
            if (ex.Count > 3)
            {
                //Commands with arguments
                if (ex.Count > 4)
                {
                    switch (command)
                    {
                        case "join":
                            if (userAuthenticated || console)
                            {
                                SendData("JOIN", Util.JoinStringList(ex, ",", 4));
                                foreach (string channel in ex.GetRange(4, ex.Count - 4))
                                {
                                    if (!Channels.ContainsKey(channel.ToLower()))
                                    {
                                        Channels.Add(channel.ToLower(), new Channel());
                                    }
                                }
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "about":
                            SendData("PRIVMSG", ex[2] + " :" + _config.About);
                            break;
                        case "help":
                            string help = "Available commands are: ";
                            help += Util.JoinStringList(_config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ");
                            help += " . With prefixes: " + Util.JoinStringList(_config.Prefixes, ", ") + " .";
                            SendData("PRIVMSG", ex[2] + " :" + help.Replace("\\", ""));
                            break;
                        case "letmegooglethatforyou":
                            SendData("PRIVMSG", ex[2] + " :http://lmgtfy.com/?q=" + Util.JoinStringList(ex, "+", 4));
                            break;
                        case "nick":
                            if (userAuthenticated || console)
                            {
                                SendData("NICK", Util.JoinStringList(ex, "_", 4));
                                _config.Server.User.Nick = Util.JoinStringList(ex, "_", 4);
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "part":
                            if (userAuthenticated || console)
                            {
                                SendData("PART", Util.JoinStringList(ex, ",", 4));
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "quit":
                            if (userAuthenticated || console)
                            {
                                SendData("QUIT", ":" + Util.JoinStringList(ex, " ", 4));
                            }
                            else
                            {
                                SendData("PRIVMSG", ":" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                    }
                }
                else //Commands without arguments
                {
                    switch (command)
                    {
                        case "about":
                            SendData("PRIVMSG", ex[2] + " :" + _config.About);
                            break;
                        case "help":
                            string help = "Available commands are: ";
                            help += Util.JoinStringList(_config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ") + ", ";
                            foreach (PluginInformation plugin in _plugins)
                            {
                                help += Util.JoinStringList(plugin.Config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ");
                            }
                            help += "; With prefixes: " + Util.JoinStringList(_config.Prefixes, ", ");
                            SendData("PRIVMSG", ex[2] + " :" + help.Replace("\\", ""));
                            break;
                        case "plugins":
                            string plugins = "Loaded plugins: ";
                            plugins += Util.JoinStringList(_plugins.Select(plugin => plugin.Name).ToList(), ", ");
                            SendData("PRIVMSG", ex[2] + " :" + plugins + ".");
                            break;
                        case "part":
                            if (userAuthenticated || console)
                            {
                                SendData("PART", ex[2]);
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "quit":
                            if (userAuthenticated || console)
                            {
                                Quit();
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                    }
                }
            }
        }

        private bool _canDoCommand(string name)
        {
            bool inList = false;
            foreach (string user in _config.Authorization.AuthorizedUsers)
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
            if (ex.Count == 6 && Util.NickRegex.Match(ex[0]).Value == "NickServ")
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

        private void Quit(string message = "")
        {
            Running = false;
            SendData("QUIT", message);

            Console.WriteLine("Disabling plugins ...");
            foreach (Kiwana.Core.Plugins.PluginInformation plugin in _plugins)
            {
                Console.Write("  - " + plugin.Name + " ... ");
                plugin.Instance.Disable();
                Console.WriteLine("OK");
            }

            Console.WriteLine("Bot stopped.");
        }

        public delegate void NewLineHandler(List<string> ex, string command, bool userAuthenticated, bool console);

        public event NewLineHandler NewLine;
    }
}
