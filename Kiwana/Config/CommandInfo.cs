using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    /// <summary>
    /// Helper Class for (de)serializing Commands.
    /// </summary>
    [XmlRoot("Command")]
    public class CommandInfo : IXmlSerializable
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The actual command information.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// <see cref="XmlSerializer"/> for the Custom (de)serializing process.
        /// </summary>
        private XmlSerializer _serializer = new XmlSerializer(typeof(Command));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo"/> class.
        /// </summary>
        public CommandInfo()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo"/> class. Used by the Commands Dictionary helper.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="command">The actual information about the command.</param>
        public CommandInfo(string name, Command command)
        {
            Name = name;
            Command = command;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Custom method to deserialize the Xml.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> used by the deserialization process.</param>
        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            reader.MoveToContent();
            Command = (Command) _serializer.Deserialize(reader);
        }

        /// <summary>
        /// Custom method to serialize the Xml.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> used by the serialization process.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            _serializer.Serialize(writer, this);
        }
    }
}
