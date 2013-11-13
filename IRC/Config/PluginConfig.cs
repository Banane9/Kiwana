using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("PluginConfig")]
    public class PluginConfig
    {
        public string ClassName { get; set; }

        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<Command> Commands { get; set; }
    }
}
