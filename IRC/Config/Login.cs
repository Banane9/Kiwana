using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Login")]
    public class Login
    {
        public string Name { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
    }
}
