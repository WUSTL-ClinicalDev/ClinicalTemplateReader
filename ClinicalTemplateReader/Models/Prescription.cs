using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    [XmlRoot("Prescription")]
    public class Prescription
    {
        [XmlElement("Item")]
        public Item[] Items { get; set; }
        [XmlElement("MeasureItem")]
        public MeasureItem[] MeasureItem { get; set; }
    }
}