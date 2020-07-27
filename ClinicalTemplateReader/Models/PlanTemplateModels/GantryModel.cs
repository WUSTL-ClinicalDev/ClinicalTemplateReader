using System.Xml.Serialization;
using VMS.TPS.Common.Model.Types;

namespace ClinicalTemplateReader
{
    public class GantryModel
    {
        public double Rtn { get; set; }
        [XmlElement(IsNullable =true)]
        public double? StopRtn { get; set; }
        public string RtnDirection { get; set; }
    }
}