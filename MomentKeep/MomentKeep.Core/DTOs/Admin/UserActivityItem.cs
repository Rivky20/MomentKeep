namespace MomentKeep.Core.DTOs.Admin
{
    public class UserActivityItem
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}