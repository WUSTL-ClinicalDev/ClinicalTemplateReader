namespace ClinicalTemplateReader
{
    public class NormalTissueModel
    {
        public bool Use { get; set; }
        public double Priority { get; set; }
        public double DistanceFromTargetBorder { get; set; }
        public double StartDose { get; set; }
        public double EndDose { get; set; }
        public double FallOff { get; set; }
        public string Mode { get; set; }
        public bool Auto { get; set; }
    }
}