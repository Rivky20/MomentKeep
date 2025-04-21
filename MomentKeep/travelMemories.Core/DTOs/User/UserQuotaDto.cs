namespace TravelMemories.Core.DTOs.User
{
    public class UserQuotaDto
    {
        public int StorageQuotaMB { get; set; }
        public int StorageUsedMB { get; set; }
        public int StorageRemainingMB { get; set; }
        public int AiQuotaTotal { get; set; }
        public int AiQuotaUsed { get; set; }
        public int AiQuotaRemaining { get; set; }
    }
}