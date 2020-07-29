namespace ClinicalTemplateReader
{
    public enum IscoenterPlacementEnum
    {
        /// <summary>
        /// At Image Origin
        /// </summary>
        AIO,
        /// <summary>
        /// At Field Target Mass Center
        /// </summary>
        AFTS,
        /// <summary>
        /// At Image Center
        /// </summary>
        AIC,
        /// <summary>
        /// At Viewing Plane Intersection
        /// </summary>
        AVPI,
        /// <summary>
        /// Relative to Field Target Mass Center
        /// </summary>
        RFTS,
        /// <summary>
        /// Relative to Image Origin
        /// </summary>
        RIO,
        /// <summary>
        /// Relative to Image Center
        /// </summary>
        RIC,
        /// <summary>
        /// Relative to Viewing Plane Intersection.
        /// </summary>
        RVPI,
        /// <summary>
        /// None of above!
        /// </summary>
        None
    }
}