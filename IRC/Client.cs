﻿using Kiwana.Core.Commands;
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

        private TcpClient _ircConnection;
        private BotConfig _config;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private List<Kiwana.Core.Plugins.KPlugin> _plugins = new List<Plugins.KPlugin>();

        public List<Channel> Channels { get; set; }

        private Regex _prefixRegex;
        private Regex _serverCommandRegex;
        private Regex _consoleCommandRegex;

        private Random random = new Random();

        private bool _shouldRun = true;

        public Client(string config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BotConfig));
            XmlReader reader = XmlReader.Create(config);
            _config = (BotConfig)serializer.Deserialize(reader);
        }

        public async Task<bool> Init()
        {
            foreach (string pluginFolder in _config.PluginFolders)
            {
                _plugins.AddRange(PluginManager.ScanPluginFolder(pluginFolder));
            }

            Console.WriteLine("Initializing Plugins ...");
            foreach (KPlugin plugin in _plugins)
            {
                Console.Write("  - " + plugin.Name + " ... ");
                try
                {
                    await Task.Run(() => plugin.Instance.Init());
                    Console.WriteLine("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failure");
                    throw new PluginInitializationFailedException("Plugin " + plugin.Name + " failed initalization.", e);
                }
            }

            _prefixRegex = new Regex(@"(?<=" + Util.JoinStringList(_config.Prefixes, "|") + ").+");

            foreach (Kiwana.Core.Plugins.KPlugin plugin in _plugins)
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

            return true;
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
                Initialized = await Init();
            }
            
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
                //Console.WriteLine(Util.JoinStringList(ex, " "));
                if (Util.NickRegex.IsMatch(ex[0]))
                {
                    Console.WriteLine(ex[2] /*+ " " + ex[1]*/ + " <" + Util.NickRegex.Match(ex[0]) + "!" + Util.NameRegex.Match(ex[0]) + "> " + Util.MessageRegex.Match(ex[3]) + " " + Util.JoinStringList(ex, " ", 4));
                }
                else if (Util.MessageRegex.IsMatch(ex[3]))
                {
                    Console.WriteLine(Util.MessageRegex.Match(ex[3]) + " " + Util.JoinStringList(ex, " ", 4));
                }
                else if (Util.MessageRegex.IsMatch(Util.JoinStringList(ex, " ", 4)))
                {
                    Console.WriteLine(Util.MessageRegex.Match(Util.JoinStringList(ex, " ", 4)));
                }
                else
                {
                    Console.WriteLine(Util.JoinStringList(ex, " "));
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

                    string regexString = "^(" + Util.JoinStringList(commandToCheck.Alias, "|") + ")$";

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
                    ex[2] = Util.NickRegex.Match(ex[0]).Value;
                }

                NewLine(ex, normalizedCommand, false, console);

                //Commands with arguments
                if (ex.Count > 4)
                {
                    switch (normalizedCommand)
                    {
                        case "join":
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                SendData("JOIN", Util.JoinStringList(ex, ",", 4));
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
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                SendData("NICK", Util.JoinStringList(ex, "_", 4));
                                _config.Server.User.Nick = Util.JoinStringList(ex, "_", 4);
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "raw":
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                if (ex[4].ToLower() == "quit")
                                {
                                    Quit(Util.JoinStringList(ex, " ", 4));
                                }
                                else
                                {
                                    SendData(Util.JoinStringList(ex, " ", 4));
                                }
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "part":
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                SendData("PART", Util.JoinStringList(ex, ",", 4));
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "quit":
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                Quit(Util.JoinStringList(ex, " ", 4));
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
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
                            help += Util.JoinStringList(_config.Commands.Where(cmd => cmd.ConsoleServer == (console ? ConsoleServer.Console : ConsoleServer.Server) || cmd.ConsoleServer == ConsoleServer.Both).Select(cmd => cmd.Name).ToList(), ", ") + ", ";
                            foreach (KPlugin plugin in _plugins)
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
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
                            {
                                SendData("PART", ex[2]);
                            }
                            else
                            {
                                SendData("PRIVMSG", ex[2] + " :" + Util.NickRegex.Match(ex[0]) + ": You don't have permission to do this.");
                            }
                            break;
                        case "quit":
                            if (_canDoCommand(Util.NickRegex.Match(ex[0]).Value) || console)
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

        private async void Quit(string message = "")
        {
            _shouldRun = false;
            SendData("QUIT", message);

            Console.WriteLine("Disabling plugins ...");
            foreach (Kiwana.Core.Plugins.KPlugin plugin in _plugins)
            {
                Console.Write("  - " + plugin.Name + " ... ");
                await Task.Run(() => plugin.Instance.Disable());
                Console.WriteLine("OK");
            }
        }

        public delegate void NewLineHandler(List<string> ex, string command, bool userAuthenticated, bool console);

        public event NewLineHandler NewLine;
    }
}
