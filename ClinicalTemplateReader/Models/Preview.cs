using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class PreviewModel
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Mode { get; set; }
        [XmlAttribute]
        public string ApprovalHistory { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string LastModified { get; set; }
        [XmlAttribute]
        public string ApprovalStatus { get; set; }
        [XmlAttribute]
        public string TreatmentSite { get; set; }
        [XmlAttribute]
        public string TreatmentStyle { get; set; }
    }
}