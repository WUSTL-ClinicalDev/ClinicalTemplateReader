using System.Xml.Serialization;
using VMS.TPS.Common.Model.Types;

namespace ClinicalTemplateReader
{
    public class MLCPlan
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public bool DynamicFlag { get; set; }
        public string ModelName { get; set; }
        public int SegmentCount { get; set; }
        public MLCMarginModel MLCMargin { get; set; }
        public string ContourMeetPoint { get; set; }
        public string ClosedMeetPoint { get; set; }
        public TargetModel Target { get; set; }
    }
}