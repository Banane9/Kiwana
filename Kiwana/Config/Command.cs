using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Stores information about a Command.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// The minimum rank of the <see cref="UserGroup"/> to access that command.
        /// </summary>
        [XmlElement("Rank")]
        public int Rank { get; set; }

        /// <summary>
        /// List of Aliases for the command.
        /// </summary>
        [XmlArray("Aliases")]
        [XmlArrayItem("Alias")]
        public List<string> Aliases { get; set; }
    }
}
