using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    //ignore for item. Only changes for MeasureItem.
    public enum TypeEnum
    {
        [XmlEnum("0")]
        ConformityIndex,//0
        [XmlEnum("1")]
        GradientMeasure,//1
        [XmlEnum("2")]
        VolumeAtRelativeDose,//2
        [XmlEnum("3")]
        VolumeAtAbsoluteDose,//3
        [XmlEnum("4")]
        DoseAtRelativeVolume,//4
        [XmlEnum("5")]
        DoseAtAbsoluteVolume,//5

    }
}