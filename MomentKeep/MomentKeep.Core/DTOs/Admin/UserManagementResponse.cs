namespace MomentKeep.Core.DTOs.Admin
{
    public class UserManagementResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public int StorageQuota { get; set; }
        public int AiQuota { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int StorageUsed { get; set; }
        public int TripCount { get; set; }
        public int ImageCount { get; set; }
    }
}