using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class Field
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public bool FixedSSD { get; set; }
        [XmlAttribute]
        public bool UsingCompensator { get; set; }
        [XmlAttribute]
        public bool UsingMLC { get; set; }
        [XmlAttribute]
        public bool Setup { get; set; }
        [XmlAttribute]
        public bool DRRVisible { get; set; }
        public string Type { get; set; }
        public TargetModel Target { get; set; }
        public string TreatmentUnit { get; set; }
        public string Technique { get; set; }
        public EnergyModel Energy { get; set; }
        public string PrimaryFluenceMode { get; set; }
        public string DRRTemplate { get; set; }
        public int DoseRate { get; set; }
        public GantryModel Gantry { get; set; }
        public CollimatorModel Collimator { get; set; }
        [XmlElement(IsNullable = true)]
        public double? TableRtn { get; set; }
        public string ToleranceTableID { get; set; }
        [XmlElement(IsNullable = true)]
        public double? Weight { get; set; }
        public FieldMarginModel FieldMargin { get; set; }
        [XmlElement("Isocenter")]
        public IsocenterModel Isocenter { get; set; }
        public MLCPlan[] MLCPlans { get; set; }
    }
}