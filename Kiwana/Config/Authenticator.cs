using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Class that stores Information for the Authentication of <see cref="User"/>.
    /// </summary>
    [XmlRoot("Authenticator")]
    public class Authenticator
    {
        /// <summary>
        /// The HostMask of the Authenticator.
        /// </summary>
        public string HostMask { get; set; }

        /// <summary>
        /// The code that means the <see cref="User"/> is authenticated.
        /// </summary>
        public string AuthenticationCode { get; set; }

        /// <summary>
        /// The position in the message at which the code is.
        /// </summary>
        public int MessagePosition { get; set; }
    }
}
