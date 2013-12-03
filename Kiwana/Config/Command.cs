using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    public class Command
    {
        [XmlElement("Rank")]
        public int Rank { get; set; }

        [XmlArray("Aliases")]
        [XmlArrayItem("Alias")]
        public List<string> Aliases { get; set; }
    }
}
