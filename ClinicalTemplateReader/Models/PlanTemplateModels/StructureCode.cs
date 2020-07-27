using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class StructureCode
    {
        [XmlAttribute]
        public string CodeScheme { get; set; }
        [XmlAttribute]
        public string Code { get; set; }
        [XmlAttribute]
        public string CodeVersion { get; set; }
    }
}