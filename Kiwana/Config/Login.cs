using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Stores login information for logging in to a server.
    /// </summary>
    [XmlRoot("Login")]
    public class Login
    {
        /// <summary>
        /// The name used to login.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The nick used to login.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// The password used to login.
        /// </summary>
        public string Password { get; set; }
    }
}
