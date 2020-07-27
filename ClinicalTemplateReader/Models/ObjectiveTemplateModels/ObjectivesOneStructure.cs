using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    //TODO Look for Structure Avoidance Mode.
    public class ObjectivesOneStructure
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string NAME { get; set; }
        [XmlAttribute]
        public bool SurfaceOnly { get; set; }
        public TargetModel StructureTarget { get; set; }
        [XmlElement(IsNullable = true)]
        public string Distance { get; set; }
        [XmlElement(IsNullable = true)]
        public int? SamplePoints { get; set; }
        [XmlElement(IsNullable = true)]
        public long? Color { get; set; }
        public string AvoidanceStructureMode { get; set; }//not sure of this
        public Objective[] StructureObjectives { get; set; }
    }
}