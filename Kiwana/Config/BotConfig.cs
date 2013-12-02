using Kiwana.Api.Config;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("BotConfig")]
    public class BotConfig
    {
        public IrcServer Server { get; set; }

        [XmlArray("PluginFolders")]
        [XmlArrayItem("PluginFolder")]
        public List<string> PluginFolders { get; set; }

        [XmlIgnore()]
        public Dictionary<string, Command> Commands { get; set; }

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

        [XmlArray("Prefixes")]
        [XmlArrayItem("Prefix")]
        public List<string> Prefixes { get; set; }

        public Permissions Permissions { get; set; }

        public string About { get; set; }

        [XmlArray("QuitMessages")]
        [XmlArrayItem("QuitMessage")]
        public List<string> QuitMessages { get; set; }

        public uint MessageInterval { get; set; }

        public BotConfig()
        {
            Commands = new Dictionary<string, Command>();
        }
    }
}
