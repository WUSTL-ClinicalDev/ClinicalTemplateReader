namespace ClinicalTemplateReader
{
    public class SiteStatistics
    {
        public SiteStatistics(string site, int count)
        {
            Site = site;
            Count = count;
        }

        public string Site { get; private set; }
        public int Count { get; private set; }
    }
}