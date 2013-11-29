using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Server")]
    public class IrcServer
    {
        [XmlAttribute("Name")]
        public string Name { get;set; }

        public string Url { get; set; }
        public int Port { get; set; }
        public User User { get; set; }
    }
}
