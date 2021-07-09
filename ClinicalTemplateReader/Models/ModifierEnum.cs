using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public enum ModifierEnum
    {
        [XmlEnum("0")]
        AtLeast,//0
        [XmlEnum("1")]
        AtMost,//1
        [XmlEnum("2")]
        MeanDoseIs,//2
        [XmlEnum("3")]
        MaxDoseIs,//3
        [XmlEnum("4")]
        MinDoseIs,//4
        [XmlEnum("5")]
        ReferencePointReceives,//5
        [XmlEnum("6")]
        Unknown,//6 --> Could not find a combination of objectives to give me 6.
        [XmlEnum("7")]
        MeanDoseIsMoreThan,//7
        [XmlEnum("8")]
        MeanDoseIsLessThan,//8
        [XmlEnum("9")]
        MinDoseIsMoreThan,//9
        [XmlEnum("10")]
        MaxDoseIsLessThan,//10
        
    }
}