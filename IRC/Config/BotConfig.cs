using Kiwana.Core.Api.Config;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("BotConfig")]
    public class BotConfig
    {
        public IrcServer Server { get; set; }

        [XmlArray("PluginFolders")]
        [XmlArrayItem("PluginFolder")]
        public List<string> PluginFolders { get; set; }

        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<Command> Commands { get; set; }

        [XmlArray("Prefixes")]
        [XmlArrayItem("Prefix")]
        public List<string> Prefixes { get; set; }

        public Permissions Permissions { get; set; }

        public string About { get; set; }

        [XmlArray("QuitMessages")]
        [XmlArrayItem("QuitMessage")]
        public List<string> QuitMessages { get; set; }
    }
}
