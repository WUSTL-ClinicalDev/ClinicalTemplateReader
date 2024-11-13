using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    public class MeasureItem
    {
        [XmlAttribute]
        public string ID { get; set; }
        public StructureCode StructureCode { get; set; }
        public TypeEnum Type { get; set; }
        public MeasureItemModifierEnum Modifier { get; set; }
        [XmlElement(IsNullable = true)]
        public double? Value{ get; set; }
        [XmlElement(IsNullable = true)]
        public double? TypeSpecifier { get; set; }
        public bool ReportDQPValueInAbsoluteUnits { get; set; }
        [XmlElement(IsNullable =true)]
        public int? Priority { get; set; }
        [XmlElement(IsNullable = true)]
        public double? VariationAcceptable { get; set; }
        [XmlElement(IsNullable =true)]
        public bool? PrimaryClinicalGoal { get; set; }
    }
}