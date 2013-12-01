using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Api.Config
{
    [XmlRoot("PluginConfig")]
    public class PluginConfig
    {
        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<Command> Commands { get; set; }
    }
}
