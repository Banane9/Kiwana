using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Authorization")]
    public class Authorization
    {
        public Authorizer Authorizer { get; set; }

        public string AuthorizationCode { get; set; }

        public int MessagePosition { get; set; }

        public int MessageLength { get; set; } 

        [XmlArray("AutherizedUsers")]
        [XmlArrayItem("User")]
        public List<string> AuthorizedUsers { get; set; }
    }
}
