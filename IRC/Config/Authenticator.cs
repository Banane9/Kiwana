﻿using System.Xml.Serialization;

namespace Kiwana.Core.Config
{
    [XmlRoot("Authenticator")]
    public class Authenticator
    {
        public string HostMask { get; set; }

        public string AuthenticationCode { get; set; }

        public int MessagePosition { get; set; }
    }
}
