using Kiwana.Config;
using Kiwana.Objects;
using Kiwana.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kiwana.Plugins;
using System.Xml.Serialization;
using System.Xml;
using System.Threading;
using System.Xml.Schema;

namespace Kiwana
{
    /// <summary>
    /// The main Class of the bot.
    /// </summary>
    public class Kiwana
    {
        /// <summary>
        /// Gets a value indicating whether the bot is running at the moment.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// The <see cref="TcpClient"/> for the IRC Server.
        /// </summary>
        private TcpClient _ircConnection;

        /// <summary>
        /// The <see cref="NetworkStream"/> of the connection.
        /// </summary>
        private NetworkStream _networkStream;

        /// <summary>
        /// The <see cref="StreamReader"/> for the stream.
        /// </summary>
        private StreamReader _streamReader;

        /// <summary>
        /// The <see cref="StreamWriter"/> for the stream.
        /// </summary>
        private StreamWriter _streamWriter;

        /// <summary>
        /// Gets a <see cref="Dictionary"/> of plugins that are loaded. Key is the name and value is the plugin class.
        /// </summary>
        public Dictionary<string, Plugin> Plugins { get; private set; }

        /// <summary>
        /// Gets a <see cref="Dictionary"/> for the channels the bot is in. Key is the name and value is the channel class.
        /// </summary>
        public Dictionary<string, Channel> Channels { get; private set; }

        /// <summary>
        /// Gets a <see cref="Dictionary"/> for the users that the bot performed a check on. 
        /// </summary>
        public Dictionary<string, User> Users { get; private set; }

        /// <summary>
        /// Gets a <see cref="Dictionary"/> for the Command aliases. Key is the normalized command and value is the Regex.
        /// </summary>
        private Dictionary<string, Regex> _commandRegexes { get; private set; }

        /// <summary>
        /// A <see cref="Regex"/> containing all the prefixes for commands.
        /// </summary>
        private Regex _prefixRegex;

        /// <summary>
        /// A <see cref="Regex"/> to check if a command will match a command at all. Contains all the normalized commands and aliases.
        /// </summary>
        private Regex _commandRegex;

        /// <summary>
        /// <see cref="Random"/> number generator. Used in for the quit message.
        /// </summary>
        private Random _random = new Random();

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the bot last sent a message to the server. Used for flood control.
        /// </summary>
        public DateTime LastSend { get; private set; }

        /// <summary>
        /// <see cref="XmlSerializer"/> for the config.
        /// </summary>
        private XmlSerializer _configSerializer = new XmlSerializer(typeof(BotConfig));

        /// <summary>
        /// <see cref="XmlSchema"/> of the config.
        /// </summary>
        private XmlSchema _configSchema = new XmlSchema();

        /// <summary>
        /// The path to the default configFile that was used when the class was constructed.
        /// </summary>
        private string _configFile;

        /// <summary>
        /// Gets the config for the bot.
        /// </summary>
        public BotConfig Config { get; private set; }

        /// <summary>
        /// Initializes a new Instance of the <see cref="Kiwana"/> class. With the configuration from a config file.
        /// </summary>
        /// <param name="config">The path to the config file.</param>
        public Kiwana(string config)
        {
            _configSchema.SourceUri = "Config/BotConfig.xsd";
            _configFile = config;

            _loadSettings(config);
        }

        /// <summary>
        /// Loads a config file.
        /// </summary>
        /// <param name="config">The path to the config file.</param>
        private void _loadSettings(string config)
        {
            XmlReader reader = XmlReader.Create(config);
            reader.Settings.Schemas.Add(_configSchema);

            Config = (BotConfig)_configSerializer.Deserialize(reader);
        }

        /// <summary>
        /// Adds the event handlers for the plugins and calls the methods for loading the plugins and generating the command Regexes.
        /// </summary>
        private void _init()
        {
            _loadPlugins();

            _prefixRegex = new Regex(@"(?<=" + Util.JoinStringList(Config.Prefixes, "|") + ").+");

            //Add all the listeners to the NewLine event
            NewLine += _handleLine;
            foreach (KeyValuePair<string, Plugin> plugin in Plugins)
            {
                plugin.Value.SendDataEvent += SendData;
                NewLine += plugin.Value.HandleLine;
            }

            _generateCommandRegexes();
        }

        /// <summary>
        /// Generates the command Regexes.
        /// </summary>
        private void _generateCommandRegexes()
        {
            string commandRegex = @"^(";

            foreach (KeyValuePair<string, Command> command in Config.Commands)
            {
                commandRegex += command.Key + "|";

                string aliases = Util.JoinStringList(command.Value.Aliases, "|");
                
                if (!string.IsNullOrEmpty(aliases))
                {
                    commandRegex += aliases + "|";
                    _commandRegexes.Add(command.Key, new Regex("^(" + aliases + ")$"));
                }
            }

            _commandRegex = new Regex(commandRegex.TrimEnd('|') + ")$");
        }

        /// <summary>
        /// Sends a line to the server.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="argument">The rest of the message.</param>
        public void SendData(MessageTypes messageType, string argument = "")
        {
            TimeSpan sinceLastSend = new DateTime() - LastSend;
            if (sinceLastSend.TotalMilliseconds < Config.MessageInterval)
            {
                Thread.Sleep((int)Config.MessageInterval - (int)sinceLastSend.TotalMilliseconds);
            }

            try
            {
                if (argument == "")
                {
                    _streamWriter.WriteLine(messageType.ToString());
                    _streamWriter.Flush();
                }
                else
                {
                    _streamWriter.WriteLine(messageType.ToString() + " " + argument);
                    _streamWriter.Flush();
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Lost connection to server unexpectedly. Attempting to reconnect.");
                Running = false;
                _connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception [" + ex.GetType() + "] " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine(messageType.ToString() + " " + argument);
            LastSend = new DateTime();
        }

        /// <summary>
        /// Connects to the server specified in the config.
        /// </summary>
        private void _connect()
        {
            Console.WriteLine("Trying to establish Connection to " + Config.Server.Url + ":" + Config.Server.Port + " ... ");
            Console.Title = "Kiwana: Connecting ...";
            try
            {
                _ircConnection = new TcpClient(Config.Server.Url, Config.Server.Port);
                _networkStream = _ircConnection.GetStream();
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream);

                SendData(MessageTypes.PASS, Config.Server.Login.Password);
                SendData(MessageTypes.NICK, Config.Server.Login.Nick);
                SendData(MessageTypes.USER, Config.Server.Login.Nick + " Owner Banane9 :" + Config.Server.Login.Name);

                Console.WriteLine("Success");
                Running = true;

                Console.Title = Config.Server.Login.Nick + " on " + Config.Server.Name;
            }
            catch
            {
                Console.WriteLine("Failure");
                Running = false;

                Console.Title = "Kiwana: Connection failed.";
            }
        }

        /// <summary>
        /// The work method. Waits for a line from the server and then calls ParseLine to process it. To be run inside its own thread.
        /// </summary>
        /// <returns>Returns a Task so it can be run as a separate Thread.</returns>
        public async Task Work()
        {
            _init();

            _connect();
            
            while(Running)
            {
                try
                {
                    string line = _streamReader.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                    {
                        ParseLine(line);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Lost connection to server unexpectedly. Attempting to reconnect.");
                    Running = false;
                    _connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception [" + ex.GetType() + "] " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Makes the alias of a command or the command into the normalized command.
        /// </summary>
        /// <param name="cmd">The command to be normalized.</param>
        /// <returns>The normalized command.</returns>
        public string GetNormalizedCommand(string cmd)
        {
            string command = "";

            //Is it a valid command from the console or from the server
            if (_commandRegex.IsMatch(cmd))
            {
                foreach (KeyValuePair<string, Command> commandToCheck in Config.Commands)
                {
                    if (cmd == commandToCheck.Key)
                    {
                        command = cmd;
                        break;
                    }

                    if (_commandRegexes.ContainsKey(commandToCheck.Key))
                    {
                        if (_commandRegexes[commandToCheck.Key].IsMatch(cmd))
                        {
                            command = commandToCheck.Key;
                        }

                        if (!String.IsNullOrEmpty(command)) { break; }
                    }
                }
            }

            return command;
        }

        /// <summary>
        /// Parses the line received from server for content the bot has to handle before passing it to the NewLine event.
        /// </summary>
        /// <param name="line">The line from the server.</param>
        /// <param name="console">Whether it came from the console.</param>
        public void ParseLine(string line, bool console = false)
        {
            //Console.WriteLine(line);
            List<string> ex = line.Split(' ').ToList();

            if (ex.Count < 2) { return; }

            if (!console)
            {
                if (ex.Count == 2)
                {
                    if (ex[0] == "PING")
                    {
                        Console.WriteLine("PING " + ex[1]);
                        SendData(MessageTypes.PONG, ex[1]);
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

                                //List for the Users in the Channel
                                List<string> userList = new List<string>(ex.Count - 6);

                                //Add the usernames from the list; first name needs to get the colon at the start stripped.
                                userList.Add(Util.MessageRegex.Match(ex[5]).Value.TrimStart(Util.ChannelUserPrefixes));
                                foreach (string userName in ex.GetRange(6, ex.Count - 6))
                                {
                                    userList.Add(userName.TrimStart(Util.ChannelUserPrefixes));
                                }

                                //Set it in the dictionary
                                Channels[ex[4].ToLower()].Users = userList;
                            }
                        }
                    }
                }

                if (ex.Count > 5)
                {
                    if (ex[1] == "NOTICE" && ex[4] == "ACC")
                    {
                        if (Util.HostMaskRegex.Match(ex[0]).Value.ToLower() == Config.Permissions.Authenticator.HostMask.ToLower())
                        {
                            string nick = Util.MessageRegex.Match(ex[3]).Value;
                            
                            if (Users.ContainsKey(nick))
                            {
                                if (Users[nick].AuthenticationRequested)
                                {
                                    Users[nick].AuthenticationRequested = false;

                                    if (ex[Config.Permissions.Authenticator.MessagePosition] == Config.Permissions.Authenticator.AuthenticationCode)
                                    {
                                        Users[nick].Rank = Config.Permissions.DefaultRank;
                                        foreach (UserGroup group in Config.Permissions.UserGroups)
                                        {
                                            if (group.Users.Contains(nick))
                                            {
                                                Users[nick].Rank = group.Rank;
                                                break;
                                            }
                                        }

                                        Console.WriteLine("User " + nick + " is authenticated and has rank [" + Users[nick].Rank + "].");
                                    }
                                    else
                                    {
                                        Console.WriteLine("User " + nick + " is not authenticated.");
                                    }
                                }
                            }
                        }
                    }
                    else if (Util.ServerRegex.IsMatch(ex[0]) && ex[2] == Config.Server.Login.Nick && !Util.MessageRegex.IsMatch(ex[4]))
                    {
                        if (Channels.ContainsKey(ex[3].ToLower()))
                        {
                            string motdSetter = ex[4];

                            if (Util.NickRegex.IsMatch(ex[4]))
                            {
                                motdSetter = Util.NickRegex.Match(ex[4]).Value;
                            }
                            
                            Channels[ex[3].ToLower()].MotdSetter = motdSetter;
                            Channels[ex[3].ToLower()].MotdSetDate = Util.UnixToDateTime(long.Parse(ex[5]));

                            Console.WriteLine("Motd of " + ex[3] + " was set by " + motdSetter + " at " + Channels[ex[3].ToLower()].MotdSetDate.Hour + ":" + Channels[ex[3].ToLower()].MotdSetDate.Minute + " on " + Channels[ex[3].ToLower()].MotdSetDate.Day + "." + Channels[ex[3].ToLower()].MotdSetDate.Month + "." + Channels[ex[3].ToLower()].MotdSetDate.Year);
                        }
                    }
                }

                if (ex.Count == 5)
                {
                    if (ex[1] == "MODE" && ex[4] == Config.Server.Login.Nick)
                    {
                        Console.WriteLine(Util.NickRegex.Match(ex[0]).Value + " set mode of " + Config.Server.Login.Nick + " in " + ex[2] + " to " + ex[3]);
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
                            Channels[ex[2].ToLower()].Users.Add(Util.NickRegex.Match(ex[0]).Value);

                            Console.WriteLine(ex[2] + " " + Util.NickRegex.Match(ex[0]).Value + " joined the channel.");
                        }
                    }
                    else if (ex[1] == "NICK")
                    {
                        string oldNick = Util.NickRegex.Match(ex[0]).Value;
                        string newNick = Util.MessageRegex.Match(ex[2]).Value;

                        if (Users.ContainsKey(oldNick))
                        {
                            Users.Add(newNick, Users[oldNick]);
                            Users.Remove(oldNick);
                        }

                        foreach (KeyValuePair<string, Channel> channel in Channels)
                        {
                            if (channel.Value.Users.Contains(oldNick))
                            {
                                channel.Value.Users.Add(newNick);
                                channel.Value.Users.Remove(oldNick);
                            }
                        }
                    }
                }

                if (ex.Count > 2)
                {
                    if (ex[1] == "PART")
                    {
                        string channel = ex[2].ToLower();
                        if (Channels.ContainsKey(channel))
                        {
                            string nick = Util.NickRegex.Match(ex[0]).Value;
                            if (Channels[channel].Users.Contains(nick))
                            {
                                Channels[channel].Users.Remove(nick);

                                Console.WriteLine(ex[2] + " " + Util.NickRegex.Match(ex[0]).Value + " left the channel.");
                            }
                        }
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
                }

                if (Util.HostMaskRegex.IsMatch(ex[0]) || console)
                {
                    string normalizedCommand = GetNormalizedCommand(command);

                    string recipient = ex[2];

                    if (ex[2] == Config.Server.Login.Nick)
                    {
                        recipient = Util.NickRegex.Match(ex[0]).Value;
                    }

                    string nick = Util.NickRegex.Match(ex[0]).Value;

                    if (!Users.ContainsKey(nick) && !console)
                    {
                        _requestAuthentication(nick);
                    }

                    bool authorized = false;

                    if (!string.IsNullOrEmpty(normalizedCommand) && !console)
                    {
                        int rank = Config.Commands[normalizedCommand].Rank;

                        authorized = Users[nick].Rank >= rank;

                        if (!authorized)
                        {
                            SendData(MessageTypes.PRIVMSG, recipient + " :" + nick + ": You aren't allowed to do this. Minimum rank is [" + rank + "] while yours is only [" + Users[nick].Rank + "]. If this was your first message, try again shortly.");
                        }
                    }

                    NewLine(ex, recipient, normalizedCommand, console ? true : authorized, console);
                }
            }
        }

        /// <summary>
        /// Sends a message to the Authentication service specified in the config requesting the status for a User.
        /// TODO: Add option to disable authentication and make the message format more modular.
        /// </summary>
        /// <param name="nick">The nick of the user to check on.</param>
        private void _requestAuthentication(string nick)
        {
            if (!Users.ContainsKey(nick))
            {
                Users.Add(nick, new User());
            }

            Users[nick].AuthenticationRequested = true;

            //Request Authentication
            SendData(MessageTypes.PRIVMSG, Util.NickRegex.Match(Config.Permissions.Authenticator.HostMask).Value + " :ACC " + nick);
        }

        /// <summary>
        /// The bot internal method that gets called from the NewLine event.
        /// </summary>
        /// <param name="ex">The line from the server, split at spaces.</param>
        /// <param name="recipient">The channel or user the message came from. Empty when from console.</param>
        /// <param name="command">The normalized command. Empty if there's no command.</param>
        /// <param name="userAuthorized">Whether the user sending the command is authorized. False if there's no command.</param>
        /// <param name="console">Whether the line came from the console.</param>
        private void _handleLine(List<string> ex, string recipient, string command, bool userAuthorized, bool console)
        {
            if (userAuthorized)
            {
                if (ex.Count > 3)
                {
                    //Commands with arguments
                    if (ex.Count > 4)
                    {
                        switch (command)
                        {
                            case "join":
                                SendData(MessageTypes.JOIN, Util.JoinStringList(ex, ",", 4));
                                foreach (string channel in ex.GetRange(4, ex.Count - 4))
                                {
                                    if (!Channels.ContainsKey(channel.ToLower()))
                                    {
                                        Channels.Add(channel.ToLower(), new Channel());
                                    }
                                }

                                break;
                            case "about":
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + Config.About);
                                break;
                            case "help":
                                string help = "Available commands are: ";
                                help += Util.JoinStringList(Config.Commands.Keys.ToList(), ", ");
                                help += " . With prefixes: " + Util.JoinStringList(Config.Prefixes, ", ") + " .";
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + help.Replace("\\", ""));
                                break;
                            case "letmegooglethatforyou":
                                SendData(MessageTypes.PRIVMSG, recipient + " :http://lmgtfy.com/?q=" + Util.JoinStringList(ex, "+", 4));
                                break;
                            case "nick":
                                SendData(MessageTypes.NICK, Util.JoinStringList(ex, "_", 4));
                                Config.Server.Login.Nick = Util.JoinStringList(ex, "_", 4);
                                break;
                            case "part":
                                SendData(MessageTypes.PART, Util.JoinStringList(ex, ",", 4));

                                foreach (string channel in ex.GetRange(4, ex.Count - 4))
                                {
                                    if (Channels.ContainsKey(channel))
                                    {
                                        Channels.Remove(channel);
                                    }
                                }

                                break;
                            case "reload":
                                switch (ex[4].ToLower())
                                {
                                    case "config":
                                        if (ex.Count > 5)
                                        {
                                            if (!string.IsNullOrEmpty(ex[5]))
                                            {
                                                SendData(MessageTypes.PRIVMSG, recipient + " :Reloading config from " + ex[5] + ".");
                                                ReloadConfig(ex[5]);
                                            }
                                        }
                                        else
                                        {
                                            SendData(MessageTypes.PRIVMSG, recipient + " :Reloading config from default config file.");
                                            ReloadConfig();
                                        }

                                        SendData(MessageTypes.PRIVMSG, recipient + " :Done!");
                                        break;
                                    case "plugins":
                                        SendData(MessageTypes.PRIVMSG, recipient + " :Reloading plugins.");
                                        ReloadPlugins();
                                        SendData(MessageTypes.PRIVMSG, recipient + " :Done!");
                                        break;
                                }

                                break;
                            case "quit":
                                Quit(Util.JoinStringList(ex, " ", 4));
                                break;
                        }
                    }
                    else //Commands without arguments
                    {
                        switch (command)
                        {
                            case "about":
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + Config.About);
                                break;
                            case "help":
                                string help = "Available commands are: ";
                                help += Util.JoinStringList(Config.Commands.Keys.ToList(), ", ");
                                help += "; With prefixes: " + Util.JoinStringList(Config.Prefixes, ", ");
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + help.Replace("\\", ""));
                                break;
                            case "plugins":
                                string plugins = "Loaded plugins: ";
                                plugins += Util.JoinStringList(Plugins.Keys.ToList(), ", ");
                                SendData(MessageTypes.PRIVMSG, recipient + " :" + plugins + ".");
                                break;
                            case "part":
                                SendData(MessageTypes.PART, ex[2]);

                                if (Channels.ContainsKey(ex[2]))
                                {
                                    Channels.Remove(ex[2]);
                                }

                                break;
                            case "reload":
                                SendData(MessageTypes.PRIVMSG, recipient + " :Reloading plugins and config from default config file.");
                                ReloadConfig();
                                ReloadPlugins();
                                SendData(MessageTypes.PRIVMSG, recipient + " :Done!");
                                break;
                            case "quit":
                                Quit();
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Makes the bot quit from the server and stops it.
        /// </summary>
        /// <param name="message">Optional quit message. If empty, a random one from the config will be used.</param>
        private void Quit(string message = "")
        {
            Running = false;

            if (string.IsNullOrEmpty(message))
            {
                SendData(MessageTypes.QUIT, ":" + Config.QuitMessages[_random.Next(0, Config.QuitMessages.Count - 1)]);
            }
            else
            {
                SendData(MessageTypes.QUIT, ":" + message);
            }

            _unloadPlugins();

            Console.WriteLine("Bot stopped.");
        }

        /// <summary>
        /// Reloads the config from a config file.
        /// </summary>
        /// <param name="config">The path to the config file. If empty, the one it was constructed with is used.</param>
        public void ReloadConfig(string config = "")
        {
            if (string.IsNullOrEmpty(config))
            {
                _loadSettings(_configFile);
            }
            else
            {
                _loadSettings(config);
            }
        }

        /// <summary>
        /// Reloads the plugins.
        /// </summary>
        public void ReloadPlugins()
        {
            _unloadPlugins();
            _loadPlugins();
        }

        /// <summary>
        /// Loads the plugins from the plugin folders in the config.
        /// </summary>
        private void _loadPlugins()
        {
            Console.WriteLine("Loading Plugins ...");

            Plugins.Clear();
            foreach (string pluginFolder in Config.PluginFolders)
            {
                Plugins.AddRange(PluginManager.ScanPluginFolder(pluginFolder));
            }

            foreach (KeyValuePair<string, Plugin> plugin in Plugins)
            {
                Console.Write("  - " + plugin.Key + " ... ");
                try
                {
                    plugin.Value.Load();
                    Console.WriteLine("OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failure");
                    throw new PluginLoadFailedException("Plugin " + plugin.Key + " failed to load.", e);
                }
            }
        }

        /// <summary>
        /// Unloads the plugins that were loaded.
        /// </summary>
        private void _unloadPlugins()
        {
            Console.WriteLine("Disabling plugins ...");
            foreach (KeyValuePair<string, Plugin> plugin in Plugins)
            {
                Console.Write("  - " + plugin.Key + " ... ");
                plugin.Value.Unload();
                Console.WriteLine("OK");
            }
        }

        /// <summary>
        /// The delegate for the NewLine event.
        /// </summary>
        /// <param name="ex">The line from the server, split at spaces.</param>
        /// <param name="recipient">The channel or user the message came from. Empty when from console.</param>
        /// <param name="command">The normalized command. Empty if there's no command.</param>
        /// <param name="userAuthorized">Whether the user sending the command is authorized. False if there's no command.</param>
        /// <param name="console">Whether the line came from the console.</param>
        public delegate void NewLineHandler(List<string> ex, string recipient, string command, bool userAuthorized, bool console);

        /// <summary>
        /// The NewLine event. Fires when the bot receives a new line from the server that contains a message sent from a user.
        /// </summary>
        public event NewLineHandler NewLine;
    }
}
