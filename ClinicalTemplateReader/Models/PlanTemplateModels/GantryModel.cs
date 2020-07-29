using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class GantryModel
    {
        public double Rtn { get; set; }
        [XmlElement(IsNullable = true)]
        public double? StopRtn { get; set; }
        public string RtnDirection { get; set; }
    }
}