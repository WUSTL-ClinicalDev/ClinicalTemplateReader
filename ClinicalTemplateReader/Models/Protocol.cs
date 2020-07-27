using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicalTemplateReader
{
    public class Protocol
    {
        public PreviewModel Preview { get; set; }
        public Phase[] Phases { get; set; }
    }
}
