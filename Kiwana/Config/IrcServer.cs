using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Stores information about a Irc Server.
    /// </summary>
    [XmlRoot("Server")]
    public class IrcServer
    {
        /// <summary>
        /// The name of the server.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get;set; }

        /// <summary>
        /// The url used to connect to the server.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The port used to connect to the server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The login information the bot uses to login to the server.
        /// </summary>
        public Login Login { get; set; }
    }
}
