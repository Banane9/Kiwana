using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Stores information about the <see cref="Authenticator"/> and <see cref="USerGroup"/>s.
    /// </summary>
    [XmlRoot("Permissions")]
    public class Permissions
    {
        /// <summary>
        /// The default rank that a <see cref="User"/> gets that is not listed in any of the <see cref="UserGroup"/>s.
        /// </summary>
        [XmlAttribute("DefaultRank")]
        public int DefaultRank { get; set; }

        /// <summary>
        /// Information about the <see cref="Authenticator"/>.
        /// </summary>
        public Authenticator Authenticator { get; set; }

        /// <summary>
        /// List of <see cref="UserGroup"/>s.
        /// </summary>
        [XmlArray("UserGroups")]
        [XmlArrayItem("UserGroup")]
        public List<UserGroup> UserGroups { get; set; }
    }
}
