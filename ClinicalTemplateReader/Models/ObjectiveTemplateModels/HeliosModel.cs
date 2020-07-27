using System.Xml.Serialization;

namespace ClinicalTemplateReader
{
    /// <summary>
    /// Attributes not currently supported:
    ///     Interpolate
    ///     UseColors
    ///     TargetAutoCrop
    ///     Aldo
    /// </summary>
    public class HeliosModel
    {
        [XmlAttribute]
        public bool DefaultFixedJaws { get; set; }
        [XmlAttribute]
        public bool Interpolate { get; set; }
        [XmlAttribute]
        public bool UseColors { get; set; }
        [XmlAttribute]
        public bool TargetAutoCrop { get; set; }
        [XmlAttribute]
        public bool Aldo { get; set; }
        public double DefaultSmoothingX { get; set; }
        public double DefaultSmoothingY { get; set; }
        public double DefaultMinimizeDose { get; set; }
        public string DefaultOptimizationType { get; set; }
        public int MaxIterations { get; set; }
        public double MaxTime { get; set; }
        public NormalTissueModel NormalTissueObjective { get; set; }
    }
}