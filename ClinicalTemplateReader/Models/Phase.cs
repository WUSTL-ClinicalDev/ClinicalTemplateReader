using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class Phase
    {
        [XmlAttribute]
        public string ID { get; set; }
        public PlanTemplate PlanTemplate { get; set; }
        public ObjectiveTemplate ObjectiveTemplate { get; set; }
    }
}