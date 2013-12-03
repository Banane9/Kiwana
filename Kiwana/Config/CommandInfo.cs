using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Kiwana.Config
{
    [XmlRoot("Command")]
    public class CommandInfo : IXmlSerializable
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        public Command Command { get; set; }

        public CommandInfo()
        { }

        public CommandInfo(string name, Command command)
        {
            Name = name;
            Command = command;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            reader.MoveToContent();
            XmlSerializer serializer = new XmlSerializer(typeof(Command));
            Command = (Command) serializer.Deserialize(reader);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            
        }
    }
}
