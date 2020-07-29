namespace ClinicalTemplateReader
{
    public class ObjectiveTemplate
    {
        public string Type { get; set; }
        public PreviewModel Preview { get; set; }
        public HeliosModel Helios { get; set; }
        public ObjectivesOneStructure[] ObjectivesAllStructures { get; set; }
    }
}
