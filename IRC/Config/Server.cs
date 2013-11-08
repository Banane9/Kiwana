using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace IRC.Config
{
    [XmlRoot("Server")]
    public class Server
    {
        public string Name {get;set;}
        public string Url { get; set; }
        public int Port { get; set; }
        public User User { get; set; }

        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public Collection<string> Commands;
    }
}
