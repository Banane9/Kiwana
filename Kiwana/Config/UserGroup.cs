using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Stores information about a Group of <see cref="User"/>s.
    /// </summary>
    [XmlRoot("UserGroup")]
    public class UserGroup
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The rank of the group. The higher, the better.
        /// </summary>
        [XmlAttribute("Rank")]
        public int Rank { get; set; }

        /// <summary>
        /// List of User Nicks in this group.
        /// </summary>
        [XmlElement("User")]
        public List<string> Users { get; set; }
    }
}
