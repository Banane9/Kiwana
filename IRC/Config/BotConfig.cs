using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace IRC.Config
{
    [XmlRoot("BotConfig")]
    public class BotConfig
    {
        [XmlArray("Servers")]
        [XmlArrayItem("Server")]
        public Collection<Server> Servers { get; set; }
    }
}
