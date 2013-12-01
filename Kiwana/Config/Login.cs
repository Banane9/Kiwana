using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("Login")]
    public class Login
    {
        public string Name { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
    }
}
