using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicalTemplateReader.Models
{
    public class DoseMetricModel
    {
        public MetricTypeEnum MetricType { get; set; }
        public OperatorEnum Operator { get; set; }
        public string StructureId { get; set; }
        public double InputValue { get; set; }
        public double InputUnit { get; set; }
        public double OutputValue { get; set; }
        public double OutputUnit { get; set; }
        public string MetricText { get; set; }
        public double TargetValue { get; set; }
        public double TargetUnit { get; set; }
    }
}
