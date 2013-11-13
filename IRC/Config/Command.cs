using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Command")]
    public class Command
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("ConsoleServer")]
        public ConsoleServer ConsoleServer { get; set; }

        [XmlElement("Alias")]
        public List<string> Alias { get; set; }
    }
}
