namespace ClinicalTemplateReader
{
    public class ApprovalStatistics
    {
        public ApprovalStatistics(string approvalStatus, int count)
        {
            ApprovalStatus = approvalStatus;
            Count = count;
        }

        public string ApprovalStatus { get; private set; }
        public int Count { get; private set; }
    }
}