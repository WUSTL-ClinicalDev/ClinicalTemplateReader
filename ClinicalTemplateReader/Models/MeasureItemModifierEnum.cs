using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public enum MeasureItemModifierEnum
    {
        [XmlEnum("0")]
        IsMoreThan,//
        [XmlEnum("1")]
        IsLessThan,//
        [XmlEnum("2")]
        Is,//
        [XmlEnum("3")]
        IsGreaterThanOrEqualTo,//
        [XmlEnum("4")]
        IsLessThanOrEqualTo,
    }
}