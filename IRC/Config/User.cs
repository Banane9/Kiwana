using System.Xml.Serialization;

namespace IRC.Config
{
    [XmlRoot("User")]
    public class User
    {
        public string Name { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
    }
}
