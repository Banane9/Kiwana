using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("UserGroup")]
    public class UserGroup
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Rank of the group; Higher is better
        /// </summary>
        [XmlAttribute("Rank")]
        public int Rank { get; set; }

        [XmlElement("User")]
        public List<string> Users { get; set; }
    }
}
