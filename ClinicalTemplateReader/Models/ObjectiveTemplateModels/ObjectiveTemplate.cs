using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
