using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public enum ObjectiveTypeEnum
    {
        [XmlEnum("0")]
        Point,
        [XmlEnum("1")]
        Line,
        [XmlEnum("2")]
        Mean,
        [XmlEnum("3")]
        GEUDs
    }
}