using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// The Toplevel class for the configuration of the bot.
    /// </summary>
    [XmlRoot("BotConfig")]
    public class BotConfig
    {
        /// <summary>
        /// Information about the <see cref="IrcServer"/> the bot is set to connect to.
        /// </summary>
        public IrcServer Server { get; set; }

        /// <summary>
        /// The paths the bot will scan to load plugins.
        /// </summary>
        [XmlArray("PluginFolders")]
        [XmlArrayItem("PluginFolder")]
        public List<string> PluginFolders { get; set; }

        /// <summary>
        /// <see cref="Dictionary"/> of Commands. The Command information is matched to the name of it.
        /// </summary>
        [XmlIgnore()]
        public Dictionary<string, Command> Commands { get; set; }

        /// <summary>
        /// Helper property for (de)serializing the Commands.
        /// </summary>
        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public CommandInfo[] CommandInfo
        {
            get
            {
                List<CommandInfo> commandInfo = new List<CommandInfo>();
                foreach (KeyValuePair<string, Command> command in Commands)
                {
                    commandInfo.Add(new CommandInfo(command.Key, command.Value));
                }

                return commandInfo.ToArray();
            }
            set
            {
                Commands.Clear();
                foreach (CommandInfo commandInfo in value)
                {
                    Commands.Add(commandInfo.Name, commandInfo.Command);
                }
            }
        }

        /// <summary>
        /// List of prefixes that the bot accepts commands from.
        /// </summary>
        [XmlArray("Prefixes")]
        [XmlArrayItem("Prefix")]
        public List<string> Prefixes { get; set; }

        /// <summary>
        /// Information about the <see cref="Permissions"/>. Stores information about the <see cref="Authenticator"/> and <see cref="UserGroups"/>s.
        /// </summary>
        public Permissions Permissions { get; set; }

        /// <summary>
        /// The about message of the bot.
        /// </summary>
        public string About { get; set; }

        /// <summary>
        /// List of quit messages, of which one is random chosen if none is specified.
        /// </summary>
        [XmlArray("QuitMessages")]
        [XmlArrayItem("QuitMessage")]
        public List<string> QuitMessages { get; set; }

        /// <summary>
        /// The minimum interval in milliseconds at which messages are send. Used for flood control.
        /// </summary>
        public uint MessageInterval { get; set; }

        /// <summary>
        /// Constructor that initializes the Commands <see cref="Dictionary"/>.
        /// </summary>
        public BotConfig()
        {
            Commands = new Dictionary<string, Command>();
        }
    }
}
