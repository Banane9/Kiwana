using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Permissions")]
    public class Permissions
    {
        public Authenticator Authenticator { get; set; }

        [XmlArray("UserGroups")]
        [XmlArrayItem("UserGroup")]
        public List<UserGroup> UserGroups { get; set; }
    }
}
