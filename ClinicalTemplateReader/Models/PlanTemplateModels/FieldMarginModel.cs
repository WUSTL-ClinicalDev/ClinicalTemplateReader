using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class FieldMarginModel
    {
        [XmlAttribute]
        public bool OptimizeCollRtnFlag { get; set; }
        [XmlAttribute]
        public bool EllipticalMarginFlag { get; set; }
        [XmlAttribute]
        public bool BEVMarginFlag { get; set; }
        [XmlElement(IsNullable =true)]
        public double? Left { get; set; }
        [XmlElement(IsNullable =true)]
        public double? Right { get; set; }
        [XmlElement(IsNullable = true)]
        public double? Top { get; set; }
        [XmlElement(IsNullable = true)]
        public double? Bottom { get; set; }
    }
}