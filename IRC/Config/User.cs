using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("User")]
    public class User
    {
        public string Name { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
    }
}
