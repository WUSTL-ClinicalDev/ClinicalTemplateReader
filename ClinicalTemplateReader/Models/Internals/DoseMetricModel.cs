using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicalTemplateReader.Models.Internals
{
    public class DoseMetricModel
    {
        public string StructureId { get; set; }
        //public TypeEnum Type { get; set; }
        public DoseMetricTypeEnum MetricType { get; set; }
        public OperatorEnum Operator { get; set; }
        public string MetricText { get; set; }
        public double InputValue { get; set; }
        public ResultUnitEnum InputUnit { get; set; }
        public double ResultValue { get; set; }
        public ResultUnitEnum ResultUnit { get; set; }
        public double TargetValue { get; set; }
        public ResultUnitEnum TargetUnit { get; set; }
        public PassResultEnum Pass { get; set; }
        public string ResultText { get; set; }
    }
}
