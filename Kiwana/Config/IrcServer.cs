using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("Server")]
    public class IrcServer
    {
        [XmlAttribute("Name")]
        public string Name { get;set; }

        public string Url { get; set; }
        public int Port { get; set; }
        public Login Login { get; set; }
    }
}
