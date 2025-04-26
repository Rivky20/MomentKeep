namespace MomentKeep.Core.DTOs.AIImage
{
    public class AIQuotaResponse
    {
        public int Total { get; set; }
        public int Used { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetDate { get; set; }
    }
}