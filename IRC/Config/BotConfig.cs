using System.Collections.Generic;
using System.Xml.Serialization;

namespace IRC.Config
{
    [XmlRoot("BotConfig")]
    public class BotConfig
    {
        public IrcServer Server { get; set; }

        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<Command> Commands { get; set; }

        [XmlArray("Prefixes")]
        [XmlArrayItem("Prefix")]
        public List<string> Prefixes { get; set; }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public List<string> Users { get; set; }

        public string About { get; set; }
    }
}
