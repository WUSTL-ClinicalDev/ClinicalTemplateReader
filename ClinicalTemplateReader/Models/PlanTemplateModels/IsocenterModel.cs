using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class IsocenterModel
    {
        [XmlAttribute]
        public double Z { get; set; }
        [XmlAttribute]
        public double Y { get; set; }
        [XmlAttribute]
        public double X { get; set; }
        [XmlAttribute]
        public IscoenterPlacementEnum Placement { get; set; }
    }
}