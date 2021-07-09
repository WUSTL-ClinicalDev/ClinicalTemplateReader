using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class Item
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public bool Primary { get; set; }
        public TypeEnum Type { get; set; }
        public ModifierEnum Modifier { get; set; }
        public double Parameter { get; set; }
        [XmlElement(IsNullable = true)]
        public double? Dose { get; set; }
        [XmlElement(IsNullable = true)]
        public double? TotalDose { get; set; }
    }
}