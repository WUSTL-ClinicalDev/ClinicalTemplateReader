using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.Types;

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
        //FieldAlignment ruels
        public PrescriptionSite PrescriptionSite { get; set; }
       //[XmlArrayItem("Field")]
       public Field[] Fields { get; set; }

    }
}
