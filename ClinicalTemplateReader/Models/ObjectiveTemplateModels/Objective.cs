using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    /// <summary>
    /// Group is an integer that allows for the grouping of line objectives.
    /// </summary>
    public class Objective
    {
        public ObjectiveTypeEnum Type { get; set; }
        public ObjectiveOperatorEnum Operator { get; set; }
        
        public double Dose { get; set; }
       [XmlElement(IsNullable =true)]
        public double? Volume { get; set; }
        
        public double Priority { get; set; }
        [XmlElement(IsNullable = true)]
        public double? ParameterA { get; set; }
        public int Group { get; set; }
    }
}   