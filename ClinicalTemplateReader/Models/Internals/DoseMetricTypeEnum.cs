using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicalTemplateReader.Models.Internals
{
    public enum DoseMetricTypeEnum
    {
        DoseAtVolume,
        VolumeAtDose,
        MaxDose,
        MinDose,
        MeanDose,
        ConformityIndex,
        GradientMeasure,
        DoseAtReferencePoint
    }
}
