using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Api.Config
{
    [XmlRoot("PluginConfig")]
    public class PluginConfig
    {
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

        public PluginConfig()
        {
            Commands = new Dictionary<string, Command>();
        }
    }
}
