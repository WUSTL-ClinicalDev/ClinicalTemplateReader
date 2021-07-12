using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    [XmlRoot("Prescription")]
    public class Prescription
    {
        //Items are labeled as Plan Objectives in the clinical protocol preview. 
        [XmlElement("Item")]
        public Item[] Items { get; set; }
        //Measure Items are labeled as Plan Measure Details in the clinical protocol preview. 
        [XmlElement("MeasureItem")]
        public MeasureItem[] MeasureItem { get; set; }
    }
}