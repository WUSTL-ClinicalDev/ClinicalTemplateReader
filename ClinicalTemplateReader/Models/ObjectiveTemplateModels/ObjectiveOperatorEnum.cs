using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public enum ObjectiveOperatorEnum
    {
        [XmlEnum("0")]
        Upper,
        [XmlEnum("1")]
        Lower,
        [XmlEnum("2")]
        Target,
        [XmlEnum("99")]
        None
    }
}