using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class PlanTemplate
    {
        [XmlElement("Preview")]
        public PreviewModel Preview { get; set; }
        [XmlElement(IsNullable = true)]
        public double? PrescribedPercentage { get; set; }
        [XmlElement(IsNullable = true)]
        public double? DosePerFraction { get; set; }
        [XmlElement(IsNullable = true)]
        public int? FractionCount { get; set; }
        [XmlElement(IsNullable =true)]
        public string TreatmentStyle { get; set; }
        //FieldAlignment rules
        public PrescriptionSite PrescriptionSite { get; set; }
        //[XmlArrayItem("Field")]
        public Field[] Fields { get; set; }
    }
}
