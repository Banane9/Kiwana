using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Authorizer")]
    public class Authorizer
    {
        public string Nick { get; set; }

        public string Name { get; set; }

        public string Host { get; set; }
    }
}
