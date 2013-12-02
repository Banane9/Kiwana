using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("Permissions")]
    public class Permissions
    {
        [XmlAttribute("DefaultRank")]
        public int DefaultRank { get; set; }

        public Authenticator Authenticator { get; set; }

        [XmlArray("UserGroups")]
        [XmlArrayItem("UserGroup")]
        public List<UserGroup> UserGroups { get; set; }
    }
}
